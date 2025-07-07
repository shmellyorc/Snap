namespace Snap.Entities.Panels;

public class GridPanel : Panel
{
	private readonly int _columns;
	private readonly int _spacing;

	public GridPanel(int columns, int spacing, params Entity[] entities) : base(entities)
	{
		_columns = columns;
		_spacing = spacing;

		UpdateLayout(entities);
	}

	public GridPanel(int columns, params Entity[] entities) : this(columns, spacing: 4, entities) { }

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

		UpdateLayout(allKids);
		base.OnDirty(state);
	}

	private void UpdateLayout(IEnumerable<Entity> children)
	{
		if (!children.Any())
		{
			Size = Vect2.Zero;
			return;
		}

		var visibleChildren = children
			.Where(x => x.IsVisible && !x.IsExiting)
			.ToList();

		if (visibleChildren.Count == 0)
		{
			Size = Vect2.Zero;
			return;
		}

		float maxWidth = visibleChildren.Max(x => x.Size.X);
		float totalWidth = _columns * maxWidth + (_columns - 1) * _spacing;
		float totalHeight = 0f;

		for (int i = 0; i < visibleChildren.Count; i++)
		{
			var child = visibleChildren[i];
			int row = i / _columns;
			int col = i % _columns;

			child.Position = new Vect2(col * (maxWidth + _spacing), row * (child.Size.Y + _spacing));

			if (row == visibleChildren.Count / _columns)
				totalHeight += child.Size.Y + _spacing;
		}

		Size = new Vect2(totalWidth, totalHeight);
	}
}

