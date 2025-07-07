namespace Snap.Logs;

/// <summary>
/// Interface for log sinks: targets that receive log entries.
/// </summary>
public interface ILogSink
{
    void Write(char value);
    void Write(string text);
    void WriteLine(string text);
    void Flush();
    void RotateIfNeeded(long upcomingBytes);
}