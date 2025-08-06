namespace Snap.Entities.Panels;

public sealed class CenterPanel : Panel
{
	public CenterPanel(params Entity[] entities) : base(entities) { }

	protected override void OnDirty(DirtyState state)
	{
		for (int i = ChildCount - 1; i >= 0; i--)
		{
			var c = GetChild<Entity>(i);

			c.Position = Size.Center(c.Size, true);
		}

		base.OnDirty(state);
	}
}
