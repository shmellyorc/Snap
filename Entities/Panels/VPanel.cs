using Snap.Screens;
using Snap.Systems;

namespace Snap.Entities.Panels;

public class VPanel : Panel
{
	private readonly int _spacing;

	public VPanel(int spacing, params Entity[] entities) : base(entities)
	{
		_spacing = spacing;

		UpdateSize(entities);
	}

	public VPanel(params Entity[] entities) : this(spacing: 4, entities) { }

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
			child.Position = new Vect2(0, offset);

			offset += child.Size.Y;
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

		float width = vChildren.Max(x => x.Size.X);
		float totalHeight = 0f;
		for (int i = 0; i < vChildren.Count; i++)
		{
			totalHeight += vChildren[i].Size.Y;
			if (i < vChildren.Count - 1)
				totalHeight += _spacing;
		}

		var newSize = new Vect2(width, totalHeight);

		if (Size != newSize)
			Size = newSize;
	}
}
