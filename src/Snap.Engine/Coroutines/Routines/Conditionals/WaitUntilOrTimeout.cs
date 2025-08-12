namespace Snap.Engine.Coroutines.Routines.Conditionals;

/// <summary>
/// An enumerator that waits until a condition is met or a timeout elapses.
/// </summary>
/// <remarks>
/// This class is typically used in coroutines to pause execution until either the condition returns <see langword="true"/>
/// or the specified timeout duration has passed.
/// </remarks>
public class WaitUntilOrTimeout : IEnumerator
{
	private readonly Func<bool> _condition;
	private readonly float _timeout;
	private float _elapsed;

	/// <summary>
	/// Gets the current item in the enumeration. Always returns <see langword="null"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaitUntilOrTimeout"/> class.
	/// </summary>
	/// <param name="condition">A function that returns <see langword="true"/> when the wait should end.</param>
	/// <param name="timeoutSeconds">The maximum time to wait, in seconds.</param>
	public WaitUntilOrTimeout(Func<bool> condition, float timeoutSeconds)
	{
		_condition = condition;
		_timeout = timeoutSeconds;
		_elapsed = 0f;
	}

	/// <summary>
	/// Advances the enumerator to the next iteration.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the timeout has not yet elapsed and the condition is still <see langword="false"/>;
	/// <see langword="false"/> if the condition is met or the timeout has elapsed.
	/// </returns>
	public bool MoveNext()
	{
		if (_condition())
			return false;

		_elapsed += Clock.Instance.DeltaTime;
		return _elapsed < _timeout;
	}

	/// <summary>
	/// Resets the enumerator. This operation is not supported.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown.</exception>
	public void Reset() => throw new NotSupportedException();
}
