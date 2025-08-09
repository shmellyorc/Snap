namespace Snap.Engine.Coroutines.Routines.Time;

public class EverySeconds : IEnumerator
{
	private readonly float _interval;
	private readonly Action _action;
	private float _elapsed;
	public object Current => null;

	public EverySeconds(float interval, Action action)
	{
		_interval = interval;
		_action = action;
		_elapsed = 0f;
	}

	public bool MoveNext()
	{
		_elapsed += Clock.Instance.DeltaTime;
		if (_elapsed >= _interval)
		{
			_elapsed -= _interval;
			_action?.Invoke();
		}
		return true; // runs forever unless manually stopped
	}

	public void Reset() => throw new NotSupportedException();
}
