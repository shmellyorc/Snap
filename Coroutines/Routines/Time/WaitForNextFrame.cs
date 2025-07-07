namespace Snap.Coroutines.Routines.Time;

public class WaitForNextFrame : IEnumerator
{
	private bool _first = true;
	public object Current => null;

	public bool MoveNext()
	{
		if (_first)
		{
			_first = false;
			return true;
		}

		return false;
	}

	public void Reset() { }
}
