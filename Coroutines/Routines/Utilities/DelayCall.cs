namespace Snap.Coroutines.Routines.Utilities;

public class DelayCall : IEnumerator
{
	private readonly float _delay;
	private readonly Action _callback;
	private float _elapsed;
    
	public object Current => null;

	public DelayCall(float delay, Action callback)
	{
		_delay = delay;
		_callback = callback;
		_elapsed = 0f;
	}
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
	public void Reset() { }
}
