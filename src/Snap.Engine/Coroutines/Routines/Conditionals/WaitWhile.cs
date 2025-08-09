namespace Snap.Engine.Coroutines.Routines.Conditionals;

/// <summary>
/// A coroutine that waits while a given condition remains true.
/// </summary>
public sealed class WaitWhile : IEnumerator
{
	private readonly Func<bool> _predicate;

	/// <summary>
	/// Gets the current value of the enumerator. Always returns <c>null</c> for <see cref="WaitWhile"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaitWhile"/> class.
	/// </summary>
	/// <param name="predicate">
	/// A function that returns <c>true</c> while the coroutine should continue waiting.
	/// The coroutine will yield until this function returns <c>false</c>.
	/// </param>
	public WaitWhile(Func<bool> predicate) => _predicate = predicate;

	/// <summary>
	/// Advances the enumerator.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the predicate is still <c>true</c> and the coroutine should keep waiting;
	/// <c>false</c> once the predicate returns <c>false</c>.
	/// </returns>
	public bool MoveNext()
	{
		// If predicate is still true, stay waiting (return true)
		// Once predicate is false, stop waiting (return false)
		return _predicate();
	}

	/// <summary>
	/// Reset is not supported for this coroutine and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException(); // no-op

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// No-op for <see cref="WaitWhile"/>.
	/// </summary>
	public void Dispose() { } // no-op
}
