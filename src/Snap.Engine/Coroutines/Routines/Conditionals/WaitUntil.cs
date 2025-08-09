namespace Snap.Engine.Coroutines.Routines.Conditionals;

/// <summary>
/// A coroutine that waits until a given condition becomes true.
/// </summary>
public sealed class WaitUntil : IEnumerator
{
	private readonly Func<bool> _predicate;

	/// <summary>
	/// Gets the current value of the enumerator. Always returns <c>null</c> for <see cref="WaitUntil"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaitUntil"/> class.
	/// </summary>
	/// <param name="predicate">
	/// A function that returns <c>true</c> when the wait condition is satisfied.
	/// The coroutine will continue yielding until this returns <c>true</c>.
	/// </param>
	public WaitUntil(Func<bool> predicate) => _predicate = predicate;

	/// <summary>
	/// Advances the enumerator.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the predicate is still <c>false</c> and the coroutine should keep waiting;
	/// <c>false</c> once the predicate returns <c>true</c>.
	/// </returns>
	public bool MoveNext()
	{
		// If predicate is still false, stay waiting (return true)
		// Once predicate is true, stop waiting (return false)
		return !_predicate();
	}

	/// <summary>
	/// Reset is not supported for this coroutine and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException(); // no-op

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// No-op for <see cref="WaitUntil"/>.
	/// </summary>

	public void Dispose() { } // no-op
}
