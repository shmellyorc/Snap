namespace Snap.Coroutines.Routines.Animations;

/// <summary>
/// Represents a generic tween animation that interpolates between two values over time.
/// </summary>
/// <typeparam name="T">The type of value to interpolate.</typeparam>
public sealed class Tween<T> : IEnumerator
{
	private readonly T _from, _to;
	private readonly float _duration;
	private readonly EaseType _type;
	private readonly Func<T, T, float, T> _lerp;
	private readonly Action<T> _onUpdate;
	private float _elapsed;

	/// <summary>
	/// Gets the current element in the collection. Always returns null for this enumerator.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tween{T}"/> class.
	/// </summary>
	/// <param name="from">The starting value of the tween.</param>
	/// <param name="to">The target value of the tween.</param>
	/// <param name="duration">The total duration of the tween in seconds.</param>
	/// <param name="type">The easing type to apply to the interpolation.</param>
	/// <param name="lerpFunc">A delegate that interpolates between two values based on a normalized progress (0 to 1).</param>
	/// <param name="onUpdate">A callback invoked each frame with the current interpolated value.</param>
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

	/// <summary>
	/// Advances the tween by one frame.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the tween is still in progress; <c>false</c> if it has completed.
	/// </returns>
	/// <remarks>
	/// This method should be called once per frame. It applies the easing function and interpolates
	/// between the <c>from</c> and <c>to</c> values using the <c>lerpFunc</c>, then invokes <c>onUpdate</c>.
	/// </remarks>
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

	/// <summary>
	/// Reset is not supported for this tween and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();
}
