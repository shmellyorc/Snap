namespace Snap.Coroutines.Routines.Time;

public sealed class WaitForFrames : IEnumerator
{
	private float _framesLeft;

	public WaitForFrames(float frames)
	{
		if (frames < 0f) frames = 0f;
		_framesLeft = frames;
	}

	public bool MoveNext()
	{
		_framesLeft--;
		return _framesLeft >= 0f;
	}

	public object Current => null;
	public void Reset() => throw new NotSupportedException();
	public void Dispose() { }
}