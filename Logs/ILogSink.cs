namespace Snap.Logs;

/// <summary>
/// Represents a log sink — a target that receives log entries from the logger,
/// such as a console window, file, or in-game overlay.
/// </summary>
/// <remarks>
/// Log sinks are used by the <see cref="Logger"/> to fan out messages to multiple destinations.
/// Each sink is responsible for managing its own output and optional rotation logic.
/// </remarks>
public interface ILogSink
{
	/// <summary>
	/// Writes a single character to the sink.
	/// </summary>
	/// <param name="value">The character to write.</param>
	void Write(char value);

	/// <summary>
	/// Writes a full string to the sink without a newline.
	/// </summary>
	/// <param name="text">The string to write.</param>
	void Write(string text);

	/// <summary>
	/// Writes a full string to the sink followed by a newline.
	/// </summary>
	/// <param name="text">The string to write.</param>
	void WriteLine(string text);

	/// <summary>
	/// Flushes any buffered output to the underlying destination.
	/// </summary>
	void Flush();

	/// <summary>
	/// Allows the sink to rotate or manage its internal storage (e.g., file rollover)
	/// based on the number of bytes about to be written.
	/// </summary>
	/// <param name="upcomingBytes">The number of bytes that will be written shortly.</param>
	void RotateIfNeeded(long upcomingBytes);
}