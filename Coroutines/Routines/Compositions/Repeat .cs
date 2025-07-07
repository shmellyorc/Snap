namespace Snap.Coroutines.Routines.Compositions;

public sealed class Repeat : IEnumerator
{
	private readonly IEnumerator _template;
	private IEnumerator _current;
	private int _count;
	public object Current => _current?.Current;
    
	public Repeat(IEnumerator routine, int count = -1)
	{
		_template = routine;
		_count = count;
		_current = Clone(routine);
	}
	public bool MoveNext()
	{
		if (_current == null) return false;
		if (_current.MoveNext()) return true;
		if (_count == 0) return false;
		if (_count > 0) _count--;
		_current = Clone(_template);
		return _current.MoveNext();
	}
	private static IEnumerator Clone(IEnumerator r)
	{
		return r;
	}

	public void Reset() => throw new NotSupportedException();
}
