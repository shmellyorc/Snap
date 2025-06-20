using System.Collections;

namespace Snap.Coroutines.Routines.Compositions;

public class WhenAll : IEnumerator
{
	private readonly List<IEnumerator> _active;

	public object Current => null;

	public WhenAll(params IEnumerator[] routines)
		=> _active = new List<IEnumerator>(routines);

	public bool MoveNext()
	{
		for (int i = _active.Count - 1; i >= 0; i--)
		{
			var r = _active[i];
			if (r == null || !r.MoveNext())
				_active.RemoveAt(i);
		}

		return _active.Count > 0;
	}

	public void Reset() => throw new NotSupportedException();
}
