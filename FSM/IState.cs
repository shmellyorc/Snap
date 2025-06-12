using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.FSM;

public interface IState<T>
{
	void OnEnter(T owner);
	void OnExit(T owner);
	void Update(T owner, float deltaTime);
}
