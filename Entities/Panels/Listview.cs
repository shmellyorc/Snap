using System.Runtime.InteropServices;

using Snap.Entities.Graphics;
using Snap.Screens;
using Snap.Systems;

namespace Snap.Entities.Panels;


public class ListviewItem : Entity
{
	private ColorRect _bar;
	private bool _selected;

	public bool Selected
	{
		get => _selected;
		set
		{
			if (_selected == value)
				return;
			_selected = value;

			if (_bar != null)
				_bar.IsVisible = _selected;
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

	public float PerItemTimeout { get; set; } = 0.255f;
	public ListviewItem SelectedItem => ChildCount > 0 ? (ListviewItem)Children[SelectedIndex] : null;
	public T SelectedItemAs<T>() where T : ListviewItem => (T)SelectedItem;
	public bool AtTop => _scrollIndex == 0;
	public int SelectedIndex => ChildCount > 0 ? _scrollIndex + _selectedIndex : 0;
	public bool AtBottom => _scrollIndex == MaxScroll;

	public Action<Listview> OnItemSelected;

	// public Listview(uint maxItems, params ListviewItem[] items) : base(items)
	// {
	// 	if (maxItems == 0)
	// 		throw new ArgumentOutOfRangeException(nameof(maxItems), "maxItems must be greater than zero.");
	// 	if (items == null || items.Length == 0)
	// 		throw new ArgumentOutOfRangeException(nameof(items), "items cannot be null or empty.");

	// 	_maxItems = maxItems;
	// 	_avgSize = ComputeAverageSize(items);

	// 	if (_avgSize.X <= 0 || _avgSize.Y <= 0)
	// 		throw new InvalidOperationException("Item size has never been set or item size is zero.");

	// 	Size = new Vect2(_avgSize.X, _avgSize.Y * maxItems);
	// }

	public Listview(uint maxItems, params ListviewItem[] items) : base(items)
	{
		if (maxItems == 0)
			throw new ArgumentOutOfRangeException(nameof(maxItems), "maxItems must be greater than zero.");
		// if (items == null || items.Length == 0)
		// 	throw new ArgumentOutOfRangeException(nameof(items), "items cannot be null or empty.");

		_maxItems = maxItems;
		_avgSize = ComputeAverageSize(items);

		if (_avgSize.X <= 0 || _avgSize.Y <= 0)
		{
			if (items.Length > 0)
				throw new InvalidOperationException("Item size has never been set or item size is zero.");
			else
				_avgSize = Vect2.One;
		}


		Size = new Vect2(_avgSize.X, _avgSize.Y * maxItems);
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
			Size = new Vect2(avgSize.X, avgSize.Y * _maxItems);
			_avgSize = avgSize;
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
		var offsetY = 0f;
		var index = 0;

		Offset = new Vect2(0, _avgSize.Y * _scrollIndex);

		for (int i = 0; i < Children.Count; i++)
		{
			var c = (ListviewItem)Children[i];

			if (!c.IsVisible)
				continue;

			c.Position = new Vect2(0, offsetY);
			c.Selected = index == _scrollIndex + _selectedIndex;

			if (c.Selected)
				OnItemSelected?.Invoke(this);

			index++;

			offsetY += _avgSize.Y;
		}

		base.OnDirty(state);
	}

	private static Vect2 ComputeAverageSize(ListviewItem[] items)
	{
		float maxW = 0f, maxH = 0f;
		foreach (var item in items)
		{
			// skip exiting/invisible if that ever applies:
			// if (!item.IsVisible || item.IsExiting) continue;

			maxW = Math.Max(maxW, item.Size.X);
			maxH = Math.Max(maxH, item.Size.Y);
		}
		return new Vect2(maxW, maxH);
	}

	public void PreviousItem()
	{
		if (ChildCount == 0 || _itemTimeout >= 0f)
			return;

		if (_selectedIndex > 0)
		{
			_selectedIndex--;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
		else if (_scrollIndex > 0)
		{
			_scrollIndex--;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
	}

	public void NextItem()
	{
		if (ChildCount == 0 || _itemTimeout >= 0f)
			return;

		if (_selectedIndex < MaxSelectedIndex)
		{
			_selectedIndex++;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
		else if (_scrollIndex < MaxScroll)
		{
			_scrollIndex++;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
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
