namespace Snap.Coroutines.Routines.Time;

/// <summary>
/// A coroutine that waits for a specified number of real-time seconds before completing.
/// </summary>
public sealed class WaitForSeconds : IEnumerator
{
	private float _remaining;

	/// <summary>
	/// Gets the current value of the enumerator.
	/// Always returns <c>null</c> for <see cref="WaitForSeconds"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaitForSeconds"/> class.
	/// </summary>
	/// <param name="seconds">
	/// The duration to wait, in seconds. Values less than zero are clamped to zero.
	/// </param>
	public WaitForSeconds(float seconds)
	{
		if (seconds < 0f) seconds = 0f;
		_remaining = seconds;
	}

	/// <summary>
	/// Advances the timer by one frame.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the coroutine should keep waiting;
	/// <c>false</c> if the specified time has elapsed.
	/// </returns>
	/// <remarks>
	/// This uses <c>Clock.Instance.DeltaTime</c> to subtract time each frame.
	/// </remarks>
	public bool MoveNext()
	{
		_remaining -= Clock.Instance.DeltaTime;
		return _remaining > 0f;
	}

	/// <summary>
	/// Reset is not supported for this coroutine and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();

	/// <summary>
	/// Performs application-defined cleanup. No-op for <see cref="WaitForSeconds"/>.
	/// </summary>
	public void Dispose() { }
}
