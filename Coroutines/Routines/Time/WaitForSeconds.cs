namespace Snap.Coroutines.Routines.Time;

public sealed class WaitForSeconds : IEnumerator
{
	private float _remaining;

	public WaitForSeconds(float seconds)
	{
		if (seconds < 0f) seconds = 0f;
		_remaining = seconds;
	}

	public bool MoveNext()
	{
		_remaining -= Clock.Instance.DeltaTime;
		return _remaining > 0f;
	}

	public object Current => null;
	public void Reset() => throw new NotSupportedException();
	public void Dispose() { }
}
