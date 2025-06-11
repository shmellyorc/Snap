using Snap.Helpers;
using Snap.Screens;

namespace Snap.Entities.Panels;

public class Panel : Entity
{
	private readonly List<Entity> _entityAdd;
	private DirtyState _state;

	protected void SetDirtyState(DirtyState state) => _state = state;

	public Panel(params Entity[] entities) =>
		_entityAdd = new List<Entity>(entities);

	public Panel() : this(Array.Empty<Entity>()) { }

	protected override void OnEnter()
	{
		AddChild(_entityAdd.ToArray());
		_entityAdd.Clear();

		base.OnEnter();
	}

	protected override void OnUpdate()
	{
		if (_state != DirtyState.None)
		{
			OnDirty(_state);

			_state = DirtyState.None;
		}

		base.OnUpdate();
	}

	protected virtual void OnDirty(DirtyState state) { }

	public new void AddChild(params Entity[] children)
	{
		base.AddChild(children);

		// Make sure all children have the screen before updating:
		CoroutineManager.Start(
			CoroutineHelpers.WaitWhileThan(
				() => children.Any(x => x._screen == null),
				() => SetDirtyState(DirtyState.Update)
			)
		);
	}

	public new bool RemoveChild(params Entity[] children)
	{
		if (base.RemoveChild(children))
		{
			SetDirtyState(DirtyState.Update);

			return true;
		}

		return false;
	}

	public new void ClearChildren()
	{
		if (Children.Count == 0)
			return;

		if (base.RemoveChild(Children.ToArray()))
			SetDirtyState(DirtyState.Update);
	}
}
