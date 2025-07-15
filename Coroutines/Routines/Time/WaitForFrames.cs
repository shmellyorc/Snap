namespace Snap.Coroutines.Routines.Time;

/// <summary>
/// A coroutine that waits for a specified number of frames before completing.
/// </summary>
public sealed class WaitForFrames : IEnumerator
{
	private float _framesLeft;

	/// <summary>
	/// Gets the current value of the enumerator.
	/// Always returns <c>null</c> for <see cref="WaitForFrames"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaitForFrames"/> class.
	/// </summary>
	/// <param name="frames">
	/// The number of frames to wait. Fractional values are allowed.
	/// Values less than zero are clamped to zero.
	/// </param>
	public WaitForFrames(float frames)
	{
		if (frames < 0f) frames = 0f;
		_framesLeft = frames;
	}

	/// <summary>
	/// Advances the enumerator by one frame.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the coroutine should continue waiting; 
	/// <c>false</c> once the specified number of frames has elapsed.
	/// </returns>
	public bool MoveNext()
	{
		_framesLeft--;
		return _framesLeft >= 0f;
	}

	/// <summary>
	/// Reset is not supported for this coroutine and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// No-op for <see cref="WaitForFrames"/>.
	/// </summary>
	public void Dispose() { }
}