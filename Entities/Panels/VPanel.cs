namespace Snap.Entities.Panels;

public class VPanel : Panel
{
	private float _spacing;
	private bool _isAutoSize = true;
	private HAlign _hAlign = HAlign.Left;
	private VAlign _vAlign = VAlign.Top;
	private bool _isDirty = true;

	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;
			_isAutoSize = false;
			_isDirty = true;
		}
	}

	public HAlign HAlign
	{
		get => _hAlign;
		set
		{
			if (_hAlign == value)
				return;
			_hAlign = value;
			_isDirty = true;
		}
	}

	public VAlign VAlign
	{
		get => _vAlign;
		set
		{
			if (_vAlign == value)
				return;
			_vAlign = value;
			_isDirty = true;
		}
	}

	public float Spacing
	{
		get => _spacing;
		set
		{
			if (_spacing == value) return;
			_spacing = value;
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	public VPanel(float spacing, params Entity[] entities) : base(entities)
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
			if (_isAutoSize)
				base.Size = Vect2.Zero;
			base.OnDirty(state);
			return;
		}

		var width = allKids.Max(x => x.Size.X);
		var height = allKids.Sum(x => x.Size.Y + _spacing) - _spacing;
		var offset = 0f;

		for (int i = 0; i < allKids.Count; i++)
		{
			var child = allKids[i];
			var eWidth = AlignHelpers.AlignWidth(Size.X, width, HAlign);
			var eHeight = AlignHelpers.AlignHeight(Size.Y, height, VAlign);

			child.Position = new Vect2(eWidth, offset + eHeight);

			offset += child.Size.Y;
			if (i < allKids.Count - 1)
				offset += _spacing;
		}

		UpdateSize(allKids);

		base.OnDirty(state);
	}

	protected override void OnUpdate()
	{
		if (_isDirty)
		{
			foreach (var e in this.GetAncestorsOfType<Panel>())
				e.SetDirtyState(DirtyState.Update | DirtyState.Sort);
			// SetDirtyState(DirtyState.Update | DirtyState.Sort); //<-- Dont add

			_isDirty = false;
		}

		base.OnUpdate();
	}

	private void UpdateSize(IEnumerable<Entity> children)
	{
		if (!_isAutoSize)
			return;
		if (!children.Any())
		{
			base.Size = Vect2.Zero;
			return;
		}

		var vChildren = children
			.Where(x => x.IsVisible && !x.IsExiting)
			.ToList();

		if (vChildren.Count == 0)
		{
			base.Size = Vect2.Zero;
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
			base.Size = newSize;
	}
}
