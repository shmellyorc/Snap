using Snap.Tweens;

namespace Snap.Coroutines.Routines.Animations;

public sealed class Tween<T> : IEnumerator
{
	private readonly T _from, _to;
	private readonly float _duration;
	private EaseType _type;
	private readonly Func<T, T, float, T> _lerp;
	private readonly Action<T> _onUpdate;
	private float _elapsed;

	public object Current => null;

	public Tween(T from, T to, float duration, EaseType type, Func<T, T, float, T> lerpFunc, Action<T> onUpdate)
	{
		_from = from;
		_to = to;
		_duration = duration;
		_onUpdate = onUpdate;
		_lerp = lerpFunc;
		_type = type;
		_elapsed = 0f;
	}
	public bool MoveNext()
	{
		if (_elapsed < _duration)
		{
			float normalized = _elapsed / _duration;            // 0→1
			float eased = Easing.Ease(_type, normalized);       // still 0→1
																// float value = MathHelpers.Lerp(_from, _to, eased);  // maps 0→1 into [from,to]
			T value = _lerp(_from, _to, eased);
			_onUpdate?.Invoke(value);
			_elapsed += Clock.Instance.DeltaTime;

			return true;
		}

		// final “to” value
		_onUpdate?.Invoke(_to);

		return false;
	}

	public void Reset() => throw new NotSupportedException();
}
