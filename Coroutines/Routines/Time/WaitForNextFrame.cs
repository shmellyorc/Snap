namespace Snap.Coroutines.Routines.Time;

/// <summary>
/// A coroutine that waits until the next frame before continuing execution.
/// </summary>
public class WaitForNextFrame : IEnumerator
{
	private bool _first = true;

	/// <summary>
	/// Gets the current value of the enumerator.
	/// Always returns <c>null</c> for <see cref="WaitForNextFrame"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Advances the enumerator by one step.
	/// </summary>
	/// <returns>
	/// <c>true</c> on the first call, causing the coroutine to wait one frame;
	/// <c>false</c> on the second call, completing the wait.
	/// </returns>
	public bool MoveNext()
	{
		if (_first)
		{
			_first = false;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Resets the enumerator back to its initial state.
	/// </summary>
	public void Reset() { }
}
