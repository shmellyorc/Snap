namespace Snap.Logs;

/// <summary>
/// A file-based log sink that writes log entries to disk with support for rolling based on file size
/// and date-based filenames. Automatically creates a new log file each day.
/// </summary>
/// <remarks>
/// This sink is intended for long-running applications where persistent logging is required.
/// Files are named by date and can be rolled over early if a size threshold is exceeded.
/// Implements <see cref="IDisposable"/> to ensure proper file stream cleanup.
/// </remarks>
public class FileLogSink : ILogSink, IDisposable
{
	private readonly string _logDirectory;
	private readonly long _maxFileSizeBytes;
	private readonly int _maxRollFiles;
	private long _currentSize;
	private StreamWriter _writer;

	private DateTime _currentDate;
	private string _fileBaseName;

	/// <summary>
	/// Initializes a new <see cref="FileLogSink"/> that writes logs to a file in the specified directory,
	/// with automatic daily rollovers and size-based rolling support.
	/// </summary>
	/// <param name="logDirectory">The directory where log files will be stored. Created if it does not exist.</param>
	/// <param name="maxFileSizeBytes">The maximum size (in bytes) a log file can reach before it rolls over.</param>
	/// <param name="maxRollFiles">The maximum number of rolled files to keep per day. Older files are deleted.</param>
	public FileLogSink(string logDirectory, long maxFileSizeBytes, int maxRollFiles)
	{
		_logDirectory = logDirectory;
		_maxFileSizeBytes = maxFileSizeBytes;
		_maxRollFiles = maxRollFiles;

		Directory.CreateDirectory(_logDirectory);
		_currentDate = DateTime.Now.Date;
		_fileBaseName = _currentDate.ToString("ddd-MMM-yyyy");
		OpenWriter();
	}

	private void OpenWriter()
	{
		if (_writer != null)
		{
			try { _writer.Flush(); } catch { /* swallow flush errors */ }
			try { _writer.Dispose(); } catch { /* swallow dispose errors */ }
		}
		string path = GetBasePath();
		_currentSize = File.Exists(path) ? new FileInfo(path).Length : 0;
		var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
		_writer = new StreamWriter(fs, Encoding.Default) { AutoFlush = true };
	}

	private string GetBasePath() => Path.Combine(_logDirectory, _fileBaseName + ".log");
	private string GetRolledPath(int idx) => Path.Combine(_logDirectory, $"{_fileBaseName}-{idx}.log");

	private void CheckDateRollover()
	{
		var today = DateTime.Now.Date;
		if (today != _currentDate)
		{
			// Safely close old writer
			try { _writer.Close(); } catch { /* swallow close errors */ }
			try { _writer.Dispose(); } catch { /* swallow dispose errors */ }

			_currentDate = today;
			_fileBaseName = _currentDate.ToString("ddd-MMM-yyyy");
			OpenWriter();
		}
	}

	/// <summary>
	/// Writes a single character to the current log file.
	/// Automatically checks for daily rollover and size-based rotation.
	/// </summary>
	/// <param name="value">The character to write.</param>
	public void Write(char value)
	{
		CheckDateRollover();
		RotateIfNeeded(1);
		_writer.Write(value);
		_currentSize++;
	}

	/// <summary>
	/// Writes a string to the current log file without a newline.
	/// Automatically checks for daily rollover and size-based rotation.
	/// </summary>
	/// <param name="text">The text to write.</param>
	public void Write(string text)
	{
		CheckDateRollover();
		var bytes = Encoding.Default.GetByteCount(text);
		RotateIfNeeded(bytes);
		_writer.Write(text);
		_currentSize += bytes;
	}

	/// <summary>
	/// Writes a string to the current log file followed by a newline.
	/// Automatically checks for daily rollover and size-based rotation.
	/// </summary>
	/// <param name="text">The line to write.</param>
	public void WriteLine(string text)
	{
		CheckDateRollover();
		var line = text + Environment.NewLine;
		var bytes = Encoding.Default.GetByteCount(line);
		RotateIfNeeded(bytes);
		_writer.WriteLine(text);
		_currentSize += bytes;
	}

	/// <summary>
	/// Flushes the underlying file writer, ensuring that all buffered log data is written to disk.
	/// </summary>
	public void Flush() => _writer.Flush();

	/// <summary>
	/// Checks whether the log file needs to be rotated based on the number of upcoming bytes to be written.
	/// If the file size would exceed the configured limit, performs a rollover and renames older log files.
	/// </summary>
	/// <param name="upcomingBytes">
	/// The number of bytes about to be written. Used to determine if rotation is required.
	/// </param>
	/// <remarks>
	/// Automatically closes and replaces the current file.  
	/// Older rolled files are renamed or deleted according to the <c>maxRollFiles</c> limit.
	/// </remarks>
	public void RotateIfNeeded(long upcomingBytes)
	{
		if (_currentSize + upcomingBytes <= _maxFileSizeBytes)
			return;

		// Safely close and dispose current writer
		try { _writer.Close(); } catch { /* swallow close errors */ }
		try { _writer.Dispose(); } catch { /* swallow dispose errors */ }

		for (int i = _maxRollFiles; i >= 1; i--)
		{
			string dst = GetRolledPath(i);
			string src = (i == 1) ? GetBasePath() : GetRolledPath(i - 1);
			try { if (File.Exists(dst)) File.Delete(dst); } catch { /* swallow delete errors */ }
			try { if (File.Exists(src)) File.Move(src, dst); } catch { /* swallow move errors */ }
		}

		OpenWriter();
	}

	/// <summary>
	/// Disposes the file sink by flushing and releasing the underlying file writer.
	/// </summary>
	/// <remarks>
	/// Any exceptions during flush or disposal are silently ignored.  
	/// This method should be called when the logger is shutting down or the sink is no longer needed.
	/// </remarks>
	public void Dispose()
	{
		try { _writer?.Flush(); } catch { }
		try { _writer?.Dispose(); } catch { }
	}
}