namespace Snap.Engine.Coroutines.Routines.Conditionals;

public class WaitUntilOrTimeout : IEnumerator
{
	private readonly Func<bool> _condition;
	private readonly float _timeout;
	private float _elapsed;
    
	public object Current => null;

	public WaitUntilOrTimeout(Func<bool> condition, float timeoutSeconds)
	{
		_condition = condition;
		_timeout = timeoutSeconds;
		_elapsed = 0f;
	}

	public bool MoveNext()
	{
		if (_condition())
			return false;

		_elapsed += Clock.Instance.DeltaTime;
		return _elapsed < _timeout;
	}

	public void Reset() => throw new NotSupportedException();
}
