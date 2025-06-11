using System.Collections;

namespace Snap.Coroutines.Routines.Conditionals;

public sealed class WaitUntil : IEnumerator
{
	private readonly Func<bool> _predicate;

	public object Current => null;

	public WaitUntil(Func<bool> predicate) => _predicate = predicate;

	public bool MoveNext()
	{
		// If predicate is still false, stay waiting (return true)
		// Once predicate is true, stop waiting (return false)
		return !_predicate();
	}

	public void Reset() => throw new NotSupportedException(); // no-op

	public void Dispose() { } // no-op
}
