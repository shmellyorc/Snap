namespace Snap.Coroutines.Routines.Conditionals;

public sealed class WaitWhile : IEnumerator
{
	private readonly Func<bool> _predicate;

	public object Current => null;

	public WaitWhile(Func<bool> predicate) => _predicate = predicate;

	public bool MoveNext()
	{
		// If predicate is still true, stay waiting (return true)
		// Once predicate is false, stop waiting (return false)
		return _predicate();
	}

	public void Reset() => throw new NotSupportedException(); // no-op

	public void Dispose() { } // no-op
}
