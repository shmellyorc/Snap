namespace Snap.Engine.Coroutines.Routines.Utilities;

/// <summary>
/// A coroutine that delays execution for a specified time, then invokes a callback once.
/// </summary>
public class DelayCall : IEnumerator
{
	private readonly float _delay;
	private readonly Action _callback;
	private float _elapsed;

	/// <summary>
	/// Gets the current value of the enumerator.
	/// Always returns <c>null</c> for <see cref="DelayCall"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="DelayCall"/> class.
	/// </summary>
	/// <param name="delay">The delay duration in seconds before the callback is invoked.</param>
	/// <param name="callback">The action to invoke after the delay.</param>
	public DelayCall(float delay, Action callback)
	{
		_delay = delay;
		_callback = callback;
		_elapsed = 0f;
	}

	/// <summary>
	/// Advances the coroutine by one frame, accumulating elapsed time until the delay is reached.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the delay has not yet completed; <c>false</c> once the callback has been invoked.
	/// </returns>
	public bool MoveNext()
	{
		if (_elapsed < _delay)
		{
			_elapsed += Clock.Instance.DeltaTime;
			return true;
		}

		_callback?.Invoke();

		return false;
	}

	/// <summary>
	/// Resets the enumerator. This method is a no-op and does not reset the delay or callback.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();
}
