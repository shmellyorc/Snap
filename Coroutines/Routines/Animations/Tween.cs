using System.Collections;

using Snap.Helpers;
using Snap.Systems;

namespace Snap.Coroutines.Routines.Animations;

public sealed class Tween : IEnumerator
{
	private readonly float _from, _to, _duration;
	private readonly Action<float> _onUpdate;
	private float _elapsed;
	private EaseType _type;

	public object Current => null;

	public Tween(float from, float to, float duration, EaseType type, Action<float> onUpdate)
	{
		_from = from;
		_to = to;
		_duration = duration;
		_onUpdate = onUpdate;
		_type = type;
		_elapsed = 0f;
	}
	public bool MoveNext()
	{
		if (_elapsed < _duration)
		{
			float normalized = _elapsed / _duration;            // 0→1
			float eased = Easing.Ease(_type, normalized);       // still 0→1
			float value = MathHelpers.Lerp(_from, _to, eased);  // maps 0→1 into [from,to]
			
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
