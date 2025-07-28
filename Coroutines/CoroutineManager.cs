/*
 
MIT License

Copyright (c) 2017 Chevy Ray Johnston

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

namespace Snap.Coroutines;

/// <summary>
/// A container for running multiple routines in parallel. Coroutines can be nested.
/// </summary>
public sealed class CoroutineManager
{
	private readonly static object PublicOwner = new();
	private readonly List<IEnumerator> _running = [];
	private readonly List<float> _delays = [];
	private readonly List<object> _owners = [];

	/// <summary>
	/// How many coroutines are currently running.
	/// </summary>
	public int Count => _running.Count;

	/// <summary>
	/// Gets the global instance of the <see cref="CoroutineManager"/>.
	/// This is typically initialized once and used to queue or update coroutines globally.
	/// </summary>
	public static CoroutineManager Instance { get; private set; }

	internal CoroutineManager() => Instance ??= this;

	/// <summary>
	/// Run a coroutine.
	/// </summary>
	/// <returns>A handle to the new coroutine.</returns>
	/// <param name="delay">How many seconds to delay before starting.</param>
	/// <param name="routine">The routine to run.</param>
	public CoroutineHandle StartDelayed(float delay, IEnumerator routine) => StartDelayed(delay, PublicOwner, routine);

	internal CoroutineHandle StartDelayed(float delay, object owner, IEnumerator routine)
	{
		_running.Add(routine);
		_delays.Add(delay);
		_owners.Add(owner);

		return new CoroutineHandle(this, routine);
	}

	/// <summary>
	/// Run a coroutine.
	/// </summary>
	/// <returns>A handle to the new coroutine.</returns>
	/// <param name="routine">The routine to run.</param>
	public CoroutineHandle Start(IEnumerator routine) => StartDelayed(0f, routine);
	internal CoroutineHandle Start(IEnumerator routine, object owner) => StartDelayed(0f, owner, routine);

	/// <summary>
	/// Stop the specified routine.
	/// </summary>
	/// <returns>True if the routine was actually stopped.</returns>
	/// <param name="routine">The routine to stop.</param>
	public bool Stop(IEnumerator routine)
	{
		int i = _running.IndexOf(routine);

		if (i < 0)
			return false;

		_running[i] = null;
		_delays[i] = 0f;
		_owners[i] = null;

		return true;
	}

	/// <summary>
	/// Stop the specified routine.
	/// </summary>
	/// <returns>True if the routine was actually stopped.</returns>
	/// <param name="routine">The routine to stop.</param>
	public bool Stop(CoroutineHandle routine) => routine.Stop();

	/// <summary>
	/// Stop all running routines.
	/// </summary>
	public void StopAll()
	{
		_running.Clear();
		_delays.Clear();
		_owners.Clear();
	}

	internal void StopAll(object owner)
	{
		for (int i = 0; i < _running.Count; i++)
		{
			if (_owners[i] == owner)
			{
				_running[i] = null;
				_delays[i] = 0f;
				_owners[i] = null;
			}
		}
	}

	internal void StopAllPublicOwner() => StopAll(PublicOwner);

	/// <summary>
	/// Check if the routine is currently running.
	/// </summary>
	/// <returns>True if the routine is running.</returns>
	/// <param name="routine">The routine to check.</param>
	public bool IsRunning(IEnumerator routine) => _running.Contains(routine);

	/// <summary>
	/// Check if the routine is currently running.
	/// </summary>
	/// <returns>True if the routine is running.</returns>
	/// <param name="routine">The routine to check.</param>
	public bool IsRunning(CoroutineHandle routine) => routine.IsRunning;

	internal void Update()
	{
		for (int i = 0; i < _running.Count; i++)
		{
			if (_delays[i] > 0f)
				_delays[i] -= Clock.Instance.DeltaTime;
			else if (_running[i] == null || !MoveNext(_running[i], i))
			{
				_running.RemoveAt(i);
				_delays.RemoveAt(i);
				_owners.RemoveAt(i--);
			}
		}
	}

	private bool MoveNext(IEnumerator routine, int index)
	{
		if (routine.Current is IEnumerator enumerator)
		{
			if (MoveNext(enumerator, index))
				return true;

			_delays[index] = 0f;
		}

		bool result = routine.MoveNext();

		if (routine.Current is float fValue)
			_delays[index] = fValue;
		else if (routine.Current is double dValue)
			_delays[index] = (float)dValue;
		else if (routine.Current is int iValue)
			_delays[index] = iValue;

		return result;
	}


}