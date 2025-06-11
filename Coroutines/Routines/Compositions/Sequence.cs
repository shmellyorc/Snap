using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.Coroutines.Routines.Compositions;

public class Sequence : IEnumerator
{
	private readonly IEnumerator[] _routines;
	private int _index;

	public object Current => _routines[_index]?.Current;

	public Sequence(params IEnumerator[] routines)
	{
		_routines = routines;
		_index = 0;
	}

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

	public void Reset() { }
}