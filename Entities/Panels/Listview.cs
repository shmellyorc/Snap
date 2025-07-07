namespace Snap.Entities.Panels;

public class ListviewItem : Entity
{
	private ColorRect _bar;
	private bool _selected;

	public virtual void OnSelected(Listview listview, int selectedIndex) { }
	public virtual void OnSelectedChanged(Listview listview, int selectedIndex) { }

	public bool Selected
	{
		get => _selected;
		set
		{
			bool oldValue = _selected;
			if (_selected == value)
				return;
			_selected = value;

			if (_bar != null)
				_bar.IsVisible = _selected;

			if (_selected)
				OnSelected((Listview)Parent, ChildIndex);

			if (_selected != oldValue)
				OnSelectedChanged(ParentAs<Listview>(), ChildIndex);
		}
	}

	public Color Color { get; set; } = Color.Blue;

	protected override void OnEnter()
	{
		if (Size.X <= 0 || Size.Y <= 0)
			throw new Exception();

		AddChild(
			_bar = new ColorRect()
			{
				Size = Size,
				Color = Color,
				IsVisible = _selected,
			}
		);

		base.OnEnter();
	}
}

public sealed class Listview : RenderTarget
{
	private Vect2 _avgSize;
	private readonly uint _maxItems;
	private int _scrollIndex, _selectedIndex;
	private float _itemTimeout;
	private int MaxScroll => Math.Max(Children.Count - (int)_maxItems, 0);
	private int MaxSelectedIndex => Children.Count <= _maxItems ? Math.Max(Children.Count - 1, 0) : (int)_maxItems - 1;
	private ListviewDirection _direction;
	private float _spacing = 0f;

	public float PerItemTimeout { get; set; } = 0.255f;
	public ListviewItem SelectedItem => ChildCount > 0 ? (ListviewItem)Children[SelectedIndex] : null;
	public T SelectedItemAs<T>() where T : ListviewItem => (T)SelectedItem;
	public bool AtTop => _scrollIndex == 0;

	public ListviewItem this[int index]
	{
		get
		{
			if (index < 0 || index >= Children.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return (ListviewItem)Children[index];
		}
	}

	public int SelectedIndex
	{
		get => ChildCount > 0 ? _scrollIndex + _selectedIndex : 0;
		set
		{
			if (ChildCount == 0)
				return;

			// clamp into [0 .. last]
			int target = Math.Clamp(value, 0, Children.Count - 1);

			// if before the current window, scroll up
			if (target < _scrollIndex)
			{
				_scrollIndex = target;
				_selectedIndex = 0;
			}
			// if past the visible window, scroll down
			else if (target > _scrollIndex + MaxSelectedIndex)
			{
				_scrollIndex = target - MaxSelectedIndex;
				_selectedIndex = MaxSelectedIndex;
			}
			// otherwise it’s inside the window
			else
			{
				_selectedIndex = target - _scrollIndex;
			}

			SetDirtyState(DirtyState.Update);
		}
	}

	public bool AtBottom => _scrollIndex == MaxScroll;

	public float Spacing
	{
		get => _spacing;
		set
		{
			if (_spacing == value) return;
			_spacing = value;
			// recalc container size when spacing changes
			RecalculateSize();
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	public ListviewDirection Direction
	{
		get => _direction;
		set
		{
			if (_direction == value) return;
			_direction = value;

			RecalculateSize();
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	public Action<Listview> OnItemSelected;

	public Listview(uint maxItems, ListviewDirection direction, params ListviewItem[] items) : base(items)
	{
		ArgumentOutOfRangeException.ThrowIfZero(maxItems);

		_maxItems = maxItems;
		_avgSize = ComputeAverageSize(items);
		if (_avgSize.X <= 0 || _avgSize.Y <= 0)
			_avgSize = Vect2.One;
		_direction = direction;
		_spacing = 0;
		Size = _avgSize;
		RecalculateSize();
	}


	private void RecalculateSize()
	{
		if (Direction == ListviewDirection.Vertical)
		{
			float totalHeight = _avgSize.Y * _maxItems + _spacing * (_maxItems - 1);
			Size = new Vect2(_avgSize.X, totalHeight);
		}
		else
		{
			float totalWidth = _avgSize.X * _maxItems + _spacing * (_maxItems - 1);
			Size = new Vect2(totalWidth, _avgSize.Y);
		}
	}


	public new void AddChild(params Entity[] children)
	{
		if (children == null || children.Length == 0)
			return;

		var c = children.OfType<ListviewItem>().ToArray();
		if (c.Length == 0)
			return;

		base.AddChild(children);

		var avgSize = ComputeAverageSize(c);
		if (avgSize != _avgSize)
		{
			_avgSize = avgSize;
			RecalculateSize();
		}

		_scrollIndex = Math.Clamp(_scrollIndex, 0, MaxScroll);
		_selectedIndex = Math.Clamp(_selectedIndex, 0, MaxSelectedIndex);

		foreach (var p in this.GetAncestorsOfType<Panel>())
			p.SetDirtyState(DirtyState.Sort | DirtyState.Update);

		SetDirtyState(DirtyState.Sort | DirtyState.Update);
	}

	public new void ClearChildren()
	{
		base.ClearChildren();

		_selectedIndex = 0;
		_scrollIndex = 0;

		SetDirtyState(DirtyState.Sort | DirtyState.Update);
	}

	public new bool RemoveChild(params Entity[] children)
	{
		var result = base.RemoveChild(children);
		if (!result)
			return false;

		_scrollIndex = Math.Min(_scrollIndex, MaxScroll);
		_scrollIndex = Math.Max(_scrollIndex, 0);

		_selectedIndex = Math.Min(_selectedIndex, MaxSelectedIndex);
		_selectedIndex = Math.Max(_scrollIndex, 0);

		SetDirtyState(DirtyState.Sort | DirtyState.Update);

		return result;
	}

	protected override void OnUpdate()
	{
		if (_itemTimeout >= 0f)
			_itemTimeout -= Clock.DeltaTime;

		base.OnUpdate();
	}

	protected override void OnDirty(DirtyState state)
	{
		float offset = 0f;
		int index = 0;

		// scroll offset along primary axis
		Offset = Direction == ListviewDirection.Vertical
			? new Vect2(0, _avgSize.Y * _scrollIndex + _spacing * _scrollIndex)
			: new Vect2(_avgSize.X * _scrollIndex + _spacing * _scrollIndex, 0);

		for (int i = 0; i < Children.Count; i++)
		{
			var c = (ListviewItem)Children[i];
			if (!c.IsVisible) continue;

			c.Position = Direction == ListviewDirection.Vertical
				? new Vect2(0, offset)
				: new Vect2(offset, 0);

			c.Selected = index == _scrollIndex + _selectedIndex;
			if (c.Selected)
			{
				OnItemSelected?.Invoke(this);
			}

			index++;

			// advance by itemSize + spacing
			offset += (Direction == ListviewDirection.Vertical
				? _avgSize.Y + _spacing
				: _avgSize.X + _spacing);
		}

		base.OnDirty(state);
	}

	private static Vect2 ComputeAverageSize(ListviewItem[] items)
	{
		float maxW = 0f, maxH = 0f;
		foreach (var item in items)
		{
			maxW = Math.Max(maxW, item.Size.X);
			maxH = Math.Max(maxH, item.Size.Y);
		}
		return new Vect2(maxW, maxH);
	}

	public void PreviousItem()
	{
		if (ChildCount == 0 || _itemTimeout >= 0f)
			return;

		// if we can move selection within the visible window…
		if (_selectedIndex > 0)
		{
			_selectedIndex--;
		}
		// otherwise, if there’s more to scroll back through…
		else if (_scrollIndex > 0)
		{
			_scrollIndex--;
		}
		else
		{
			// at absolute start—nothing to do
			return;
		}

		_itemTimeout += PerItemTimeout;
		SetDirtyState(DirtyState.Update);
	}

	public void NextItem()
	{
		if (ChildCount == 0 || _itemTimeout >= 0f)
			return;

		// if we can move selection forward within the visible window…
		if (_selectedIndex < MaxSelectedIndex)
		{
			_selectedIndex++;
		}
		// otherwise, if there’s more content to scroll into view…
		else if (_scrollIndex < MaxScroll)
		{
			_scrollIndex++;
		}
		else
		{
			// at absolute end—nothing to do
			return;
		}

		_itemTimeout += PerItemTimeout;
		SetDirtyState(DirtyState.Update);
	}

	public TEnum GetSelectedIndexAsEnum<TEnum>() where TEnum : struct, Enum
	{
		int idx = SelectedIndex;
		int length = Enum.GetValues<TEnum>().Length;

		if (idx < 0 || idx >= length)
			throw new InvalidOperationException(
				$"SelectedIndex {idx} is outside the range of enum {typeof(TEnum).Name} (0-{length - 1}).");

		// cast via object to satisfy the compiler
		return (TEnum)(object)idx;
	}
}
