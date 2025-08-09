namespace Snap.Engine.Coroutines.Routines.Compositions;

/// <summary>
/// Executes a sequence of <see cref="IEnumerator"/> routines in order, one after the other.
/// </summary>
public class Sequence : IEnumerator
{
	private readonly IEnumerator[] _routines;
	private int _index;

	/// <summary>
	/// Gets the current value yielded by the active routine in the sequence.
	/// </summary>
	public object Current => _routines[Math.Clamp(_index, 0, _routines.Length - 1)]?.Current;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sequence"/> class.
	/// </summary>
	/// <param name="routines">An array of IEnumerators to run one after another.</param>
	/// <remarks>
	/// Null routines are skipped automatically.
	/// </remarks>
	public Sequence(params IEnumerator[] routines)
	{
		_routines = routines;
		_index = 0;
	}

	/// <summary>
	/// Advances the enumerator to the next step in the sequence.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the sequence is still executing routines; <c>false</c> if all routines have completed.
	/// </returns>
	public bool MoveNext()
	{
		while (_index < _routines.Length)
		{
			var r = _routines[_index];

			if (r != null && r.MoveNext())
				return true;

			_index++;
		}
		return false;
	}

	/// <summary>
	/// Reset is not supported for this sequence and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();
}