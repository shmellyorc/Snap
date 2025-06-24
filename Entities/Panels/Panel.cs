using Snap.Helpers;
using Snap.Screens;

namespace Snap.Entities.Panels;

public class Panel : Entity
{
	private readonly List<Entity> _entityAdd;
	private DirtyState _state;

	public void SetDirtyState(DirtyState state) => _state |= state;

	public Panel(params Entity[] entities)
	{
		if (entities == null || entities.Length == 0)
			return;

		_entityAdd = new List<Entity>(entities);
	}

	public Panel() : this([]) { }

	protected override void OnEnter()
	{
		if (_entityAdd != null)
		{
			base.AddChild([.. _entityAdd]);
			_entityAdd.Clear();

			SetDirtyState(DirtyState.Update | DirtyState.Sort);
		}

		base.OnEnter();
	}

	protected override void OnUpdate()
	{
		if (_state != DirtyState.None)
		{
			OnDirty(_state);

			_state = DirtyState.None;
		}
	}

	protected virtual void OnDirty(DirtyState state) { }

	public new void AddChild(params Entity[] children)
	{
		if (children == null || children.Length == 0)
			return;

		base.AddChild(children);

		SetDirtyState(DirtyState.Update | DirtyState.Sort);
	}

	public new bool RemoveChild(params Entity[] children)
	{
		if (children == null || children.Length == 0) return false;

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

		if (base.RemoveChild([.. Children]))
			SetDirtyState(DirtyState.Update);
	}
}
