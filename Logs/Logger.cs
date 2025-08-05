namespace Snap.Logs;

/// <summary>
/// Represents the severity level of a log message.
/// </summary>
public enum LogLevel
{
	/// <summary>General informational messages.</summary>
	Info,

	/// <summary>Messages indicating potential issues or unusual behavior.</summary>
	Warning,

	/// <summary>Messages representing errors or failures that require attention.</summary>
	Error,
}

/// <summary>
/// Central logging system for the engine. Supports writing log messages to multiple output targets (sinks),
/// such as console, file, or in-game debug UI. Inherits from <see cref="TextWriter"/> for compatibility
/// with standard stream APIs.
/// </summary>
/// <remarks>
/// Provides structured logging with severity levels and optional prefix formatting.  
/// All log messages are automatically dispatched to all registered sinks.
/// </remarks>
public sealed class Logger : TextWriter, IDisposable
{
    private readonly List<ILogSink> _sinks = new List<ILogSink>();
    private readonly Queue<string> _recentEntries;
    private readonly int _maxRecentEntries;
    private readonly object _syncLock = new object();
    private bool _disposed;

	/// <summary>
	/// Singleton instance of the global <see cref="Logger"/> used throughout the engine.
	/// </summary>
	public static Logger Instance { get; private set; }

	/// <summary>
	/// Gets the character encoding used by the logger when writing text output.
	/// </summary>
	public override Encoding Encoding => Encoding.Default;

	/// <summary>
	/// Gets or sets the minimum log level. Messages below this level will be ignored.
	/// </summary>
	/// <remarks>
	/// For example, setting this to <see cref="LogLevel.Warning"/> will suppress all <see cref="LogLevel.Info"/> logs.
	/// </remarks>
	public LogLevel Level { get; set; }

	/// <summary>
	/// Initializes the global <see cref="Logger"/> instance with a minimum log level and optional recent-entry tracking.
	/// </summary>
	/// <param name="minLevel">
	/// The minimum <see cref="LogLevel"/> required for a message to be logged. Defaults to <see cref="LogLevel.Info"/>.
	/// </param>
	/// <param name="maxRecentEntries">
	/// The maximum number of recent log entries to retain in memory. Defaults to <c>100</c>.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the logger has already been initialized. Only one instance is allowed.
	/// </exception>
	public Logger(LogLevel minLevel = LogLevel.Info,
                  int maxRecentEntries = 100)
    {
        if (Instance != null)
            throw new InvalidOperationException("Logger already initialized.");
        Instance = this;
        Level = minLevel;
        _maxRecentEntries = maxRecentEntries;
        _recentEntries = new Queue<string>(maxRecentEntries);
    }

	/// <summary>
	/// Adds a new log sink to the logger, such as a console output, file writer, or custom destination.
	/// </summary>
	/// <param name="sink">The <see cref="ILogSink"/> instance to receive log output.</param>
	/// <exception cref="ObjectDisposedException">
	/// Thrown if the logger has already been disposed.
	/// </exception>
	/// <remarks>
	/// Sinks are written to in the order they are added. Thread-safe.
	/// </remarks>
	public void AddSink(ILogSink sink)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Logger));
        lock (_syncLock) { _sinks.Add(sink); }
    }

	/// <summary>
	/// Writes a single character to all registered log sinks.
	/// </summary>
	/// <param name="value">The character to write.</param>
	/// <remarks>
	/// Automatically rotates each sink if needed before writing.  
	/// If the logger has been disposed, this call has no effect.
	/// </remarks>
	public override void Write(char value)
    {
        if (_disposed) return;
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(1); s.Write(value); }
        }
    }

	/// <summary>
	/// Writes a string directly to all registered log sinks without appending a newline.
	/// </summary>
	/// <param name="value">The string to write.</param>
	/// <remarks>
	/// Each sink is checked for rotation before writing.  
	/// If the logger has been disposed, this method does nothing.  
	/// Thread-safe.
	/// </remarks>
	public override void Write(string value)
    {
        if (_disposed) return;
        var bytes = Encoding.Default.GetByteCount(value);
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(bytes); s.Write(value); }
        }
    }

	/// <summary>
	/// Writes a string to all registered log sinks and appends a newline.
	/// The entry is also added to the recent log buffer for in-memory access.
	/// </summary>
	/// <param name="value">The line to write.</param>
	/// <remarks>
	/// Each sink is checked for rotation before writing.  
	/// Retains up to the configured maximum number of recent entries in memory.  
	/// Thread-safe. No action is taken if the logger is disposed.
	/// </remarks>
	public override void WriteLine(string value)
    {
        if (_disposed) return;
        var line = value;
        var bytes = Encoding.Default.GetByteCount(line + Environment.NewLine);
        lock (_syncLock)
        {
            foreach (var s in _sinks) { s.RotateIfNeeded(bytes); s.WriteLine(line); }
            _recentEntries.Enqueue(line);
            if (_recentEntries.Count > _maxRecentEntries) _recentEntries.Dequeue();
        }
    }

	/// <summary>
	/// Flushes all registered log sinks, ensuring that any buffered output is written immediately.
	/// </summary>
	/// <remarks>
	/// Thread-safe. If the logger is disposed, this method does nothing.
	/// </remarks>
	public override void Flush()
    {
        if (_disposed) return;
        lock (_syncLock)
        { foreach (var s in _sinks) s.Flush(); }
    }

	/// <summary>
	/// Logs a formatted message at the specified log level.
	/// The message is timestamped and prefixed based on severity, then written to all sinks and the debug output.
	/// </summary>
	/// <param name="level">The severity level of the log message.</param>
	/// <param name="message">The message to log.</param>
	/// <remarks>
	/// Messages below the current <see cref="Level"/> setting are ignored.  
	/// The output is written using <see cref="WriteLine"/> and also forwarded to <see cref="System.Diagnostics.Debug.WriteLine"/>.
	/// </remarks>
	public void Log(LogLevel level, string message)
    {
        if (_disposed || level < Level) return;
        string prefix = level switch { LogLevel.Warning => "[⚠️ WARNING]", LogLevel.Error => "[❌ ERROR]", _ => "[INFO]" };
        string entry = $"{DateTime.Now:HH:mm:ss} {prefix}: {message}";
        WriteLine(entry);
        Debug.WriteLine(entry);
    }

	/// <summary>
	/// Logs an exception and its inner exceptions recursively at the specified log level.
	/// Includes both the exception message and stack trace, if available.
	/// </summary>
	/// <param name="ex">The exception to log.</param>
	/// <param name="level">
	/// The severity level for the log entries. Defaults to <see cref="LogLevel.Error"/>.
	/// </param>
	/// <remarks>
	/// Prevents duplicate logging of circular exception chains using a visited set.  
	/// Skips logging if the logger is disposed or if the level is below the current <see cref="Level"/>.
	/// </remarks>
	public void LogException(Exception ex, LogLevel level = LogLevel.Error)
    {
        if (_disposed || level < Level) return;
        var seen = new HashSet<Exception>();
        var current = ex;
        while (current != null && !seen.Contains(current))
        {
            seen.Add(current);
            Log(level, $"Exception: {current.Message}");
            if (!string.IsNullOrEmpty(current.StackTrace)) Log(level, current.StackTrace);
            current = current.InnerException;
        }
    }

	/// <summary>
	/// Logs a series of key-value pairs on a single line at the specified log level.
	/// Useful for structured or contextual logging.
	/// </summary>
	/// <param name="level">The severity level of the log entry.</param>
	/// <param name="fields">
	/// A collection of fields to log, each consisting of a string key and an associated value.
	/// </param>
	/// <remarks>
	/// Fields are output as <c>Key=Value</c> pairs, separated by spaces.  
	/// Skips logging if the logger is disposed or the level is below the current <see cref="Level"/>.
	/// </remarks>
	public void LogFields(LogLevel level, params (string Key, object Value)[] fields)
    {
        if (_disposed || level < Level) return;
        var sb = new StringBuilder();
        for (int i = 0; i < fields.Length; i++)
        {
            var (Key, Value) = fields[i]; sb.Append(Key).Append('=').Append(Value);
            if (i < fields.Length - 1) sb.Append(' ');
        }
        Log(level, sb.ToString());
    }

	/// <summary>
	/// Returns a snapshot of the most recent log entries retained in memory.
	/// </summary>
	/// <returns>
	/// An array of recent log entry strings, ordered from oldest to newest.
	/// </returns>
	/// <remarks>
	/// Thread-safe. The number of retained entries is limited by the value provided at logger initialization.
	/// </remarks>
	public string[] GetRecentEntries()
    {
        lock (_syncLock) { return _recentEntries.ToArray(); }
    }

	/// <summary>
	/// Releases resources used by the logger and disposes all registered sinks that implement <see cref="IDisposable"/>.
	/// </summary>
	/// <param name="disposing">
	/// <c>true</c> to dispose managed resources; otherwise, <c>false</c>.
	/// </param>
	/// <remarks>
	/// This method is automatically called by <see cref="Dispose"/> and should not be called directly.
	/// </remarks>
	protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            lock (_syncLock)
            { foreach (var s in _sinks) if (s is IDisposable d) d.Dispose(); _sinks.Clear(); _disposed = true; Instance = null; }
        }
        base.Dispose(disposing);
    }

	/// <summary>
	/// Disposes the logger and all its sinks. After disposal, logging operations are ignored.
	/// </summary>
	public void Dispose() => Dispose(true);
}