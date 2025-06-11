using System.Collections;

using Snap.Coroutines.Routines.Conditionals;
using Snap.Coroutines.Routines.Time;

namespace Snap.Helpers;

public static class CoroutineHelpers
{
	public static IEnumerator WaitWhileThan(Func<bool> predicate, Action action)
	{
		yield return new WaitWhile(predicate);

		action();
	}

	public static IEnumerator WaitUntilThan(Func<bool> predicate, Action action)
	{
		yield return new WaitUntil(predicate);

		action();
	}

	public static IEnumerator WaitThan(float seconds, Action action)
	{
		yield return new WaitForSeconds(seconds);

		action();
	}

	public static IEnumerator Sequence(params IEnumerator[] routines)
	{
		foreach (var r in routines)
		{
			while(r.MoveNext())
				yield return null;
		}
	}

	public static IEnumerator Parallel(params IEnumerator[] routines)
	{
		bool anyRunning;
		
		do
		{
			anyRunning = false;

			foreach (var e in routines)
			{
				if (e.MoveNext())
					anyRunning = true;
			}

			yield return null;

		} while (anyRunning);
	}
}
