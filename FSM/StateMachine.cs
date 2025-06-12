namespace Snap.FSM;

public sealed class StateMachine<T>
{
	public T Owner { get; }
	public IState<T> Current { get; private set; }

	public StateMachine(T owner) => Owner = owner;

	public void ChangeState(IState<T> newState)
	{
		Current?.OnExit(Owner);
		Current = newState;
		Current?.OnEnter(Owner);
	}

	public void Update(float dt) => Current?.Update(Owner, dt);
}
