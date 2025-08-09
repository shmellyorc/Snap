namespace Snap.Engine.FSM;

/// <summary>
/// Defines the lifecycle methods for a single state in a <see cref="StateMachine{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the object that owns and drives this state machine.</typeparam>
public interface IState<T>
{
	/// <summary>
	/// Called once when the state becomes active.
	/// </summary>
	/// <param name="owner">The instance of <typeparamref name="T"/> that owns this state machine.</param>
	void OnEnter(T owner);

	/// <summary>
	/// Called once when the state is about to be replaced by another state.
	/// </summary>
	/// <param name="owner">The instance of <typeparamref name="T"/> that owns this state machine.</param>
	void OnExit(T owner);

	/// <summary>
	/// Called every update tick while this state is active.
	/// </summary>
	/// <param name="owner">The instance of <typeparamref name="T"/> that owns this state machine.</param>
	/// <param name="deltaTime">Time elapsed since the last update, in seconds.</param>
	void Update(T owner, float deltaTime);
}
