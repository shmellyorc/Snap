using Snap.Screens;
using Snap.Systems;

namespace Snap.Entities.Panels;

public class HPanel : Panel
{
	private readonly int _spacing;

	public HPanel(int spacing, params Entity[] entities) : base(entities)
	{
		_spacing = spacing;

		UpdateSize(entities);
	}

	public HPanel(params Entity[] entities) : this(spacing: 4, entities) { }

	protected override void OnDirty(DirtyState state)
	{
		var allKids = Children
			.Where(x => x.IsVisible && !x.IsExiting)
			.ToList();

		if (allKids.Count == 0)
		{
			Size = Vect2.Zero;
			base.OnDirty(state);
			return;
		}

		var offset = 0f;
		for (int i = 0; i < allKids.Count; i++)
		{
			var child = allKids[i];
			child.Position = new Vect2(offset, 0);

			offset += child.Size.X;
			if (i < allKids.Count - 1)
				offset += _spacing;
		}

		UpdateSize(allKids);

		base.OnDirty(state);
	}

	private void UpdateSize(IEnumerable<Entity> children)
	{
		if (!children.Any())
		{
			Size = Vect2.Zero;
			return;
		}

		var vChildren = children
			.Where(x => x.IsVisible && !x.IsExiting)
			.ToList();

		if (vChildren.Count == 0)
		{
			Size = Vect2.Zero;
			return;
		}

		float height = vChildren.Max(x => x.Size.Y);
		float totalWidth = 0f;
		for (int i = 0; i < vChildren.Count; i++)
		{
			totalWidth += vChildren[i].Size.X;
			if (i < vChildren.Count - 1)
				totalWidth += _spacing;
		}

		var newSize = new Vect2(totalWidth, height);

		if (Size != newSize)
			Size = newSize;
	}
}
