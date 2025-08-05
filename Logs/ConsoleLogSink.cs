namespace Snap.Logs;

/// <summary>
/// A console-based log sink that writes log output to standard console output (stdout).
/// Useful for debugging, CLI tools, or development builds where real-time log visibility is needed.
/// </summary>
/// <remarks>
/// This sink writes directly to <see cref="System.Console"/> and supports flushing,
/// but does not implement file rotation or persistent storage.
/// </remarks>
public class ConsoleLogSink : ILogSink
{
	/// <summary>
	/// Writes a single character to the console output.
	/// </summary>
	/// <param name="value">The character to write.</param>
	public void Write(char value) => Console.Write(value);

	/// <summary>
	/// Writes a string to the console output without appending a newline.
	/// </summary>
	/// <param name="text">The text to write.</param>
	public void Write(string text) => Console.Write(text);

	/// <summary>
	/// Writes a string to the console output followed by a newline.
	/// </summary>
	/// <param name="text">The line of text to write.</param>
	public void WriteLine(string text) => Console.WriteLine(text);

	/// <summary>
	/// Flushes the console output buffer.
	/// </summary>
	public void Flush() => Console.Out.Flush();

	/// <summary>
	/// No-op for console output. Rotation is not applicable for this sink.
	/// </summary>
	/// <param name="upcomingBytes">Unused. Included to satisfy the <see cref="ILogSink"/> interface.</param>
	public void RotateIfNeeded(long upcomingBytes) { /* no-op for console */ }
}