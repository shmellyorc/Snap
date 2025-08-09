namespace Snap.Engine.Coroutines.Routines.Compositions;

/// <summary>
/// Repeats a given <see cref="IEnumerator"/> a specified number of times, or infinitely if count is negative.
/// </summary>
public sealed class Repeat : IEnumerator
{
	private readonly Func<IEnumerator> _factory;
	private IEnumerator _current;
	private int _count;

	/// <summary>
	/// Gets the current value yielded by the active iteration of the repeated routine.
	/// </summary>
	public object Current => _current?.Current;

	/// <summary>
	/// Initializes a new instance of the <see cref="Repeat"/> class.
	/// </summary>
	/// <param name="factory">A function that creates a new IEnumerator instance each time it's called.</param>
	/// <param name="count">
	/// The number of times to repeat the routine.
	/// A negative value (default -1) causes it to repeat indefinitely.
	/// </param>
	public Repeat(Func<IEnumerator> factory, int count = -1)
	{
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		_count = count;
		_current = _factory();
	}

	/// <summary>
	/// Advances the enumerator to the next element in the sequence.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the routine is still running; <c>false</c> if all repetitions are complete.
	/// </returns>
	public bool MoveNext()
	{
		if (_current == null)
			return false;

		if (_current.MoveNext())
			return true;

		if (_count == 0)
			return false;

		if (_count > 0)
			_count--;

		_current = _factory();
		return _current.MoveNext();
	}

	/// <summary>
	/// Reset is not supported for this enumerator and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();
}

