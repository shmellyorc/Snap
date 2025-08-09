namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A layout panel that arranges its child entities in a uniform grid, with a fixed number of columns and configurable spacing.
/// </summary>
public class GridPanel : Panel
{
	private readonly int _columns;
	private readonly int _spacing;

	/// <summary>
	/// Initializes a new <see cref="GridPanel"/> with a specific number of columns, spacing between elements, and optional child entities.
	/// </summary>
	/// <param name="columns">The number of columns in the grid layout.</param>
	/// <param name="spacing">The horizontal and vertical spacing (in pixels) between grid cells.</param>
	/// <param name="entities">The child entities to add to the panel.</param>
	public GridPanel(int columns, int spacing, params Entity[] entities) : base(entities)
	{
		_columns = columns;
		_spacing = spacing;

		UpdateLayout(entities);
	}

	/// <summary>
	/// Initializes a new <see cref="GridPanel"/> with a specific number of columns, using a default spacing of 4 pixels.
	/// </summary>
	/// <param name="columns">The number of columns in the grid layout.</param>
	/// <param name="entities">The child entities to add to the panel.</param>
	public GridPanel(int columns, params Entity[] entities) : this(columns, spacing: 4, entities) { }

	/// <summary>
	/// Called when the layout needs to be updated due to changes in child visibility, position, or structure.
	/// Recalculates the size and positions of all visible children in the grid.
	/// </summary>
	/// <param name="state">The dirty state triggering the layout update.</param>
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

		foreach (var e in this.GetAncestorsOfType<Panel>())
			e.SetDirtyState(DirtyState.Update | DirtyState.Sort);

		if (IsTopmostScreen || Parent == null)
			Screen?.SetDirtyState(DirtyState.Sort | DirtyState.Update);

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

