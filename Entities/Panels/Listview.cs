using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Snap.Coroutines.Routines.Conditionals;
using Snap.Entities.Graphics;
using Snap.Screens;
using Snap.Systems;

namespace Snap.Entities.Panels;


public class ListviewItem : Entity
{
	private ColorRect _bar;
	private bool _selected;
	private bool _isDirty;


	public bool Selected
	{
		get => _selected;
		set
		{
			if (_selected == value)
				return;
			_selected = value;

			_isDirty = true;
		}
	}

	public Color Color { get; set; } = Color.Blue;

	protected override void OnEnter()
	{
		if (Size.X <= 0 || Size.Y <= 0)
			throw new Exception();

		AddChild(
			_bar = new ColorRect() { Size = Size, Color = Color, IsVisible = _selected }
		);

		base.OnEnter();
	}

	protected override void OnUpdate()
	{
		if (_bar == null)
		{
			base.OnUpdate();
			return;
		}

		if (_isDirty)
		{
			_bar.IsVisible = _selected;
			_isDirty = false;
		}

		base.OnUpdate();
	}
}


public sealed class Listview : RenderTarget
{
	private Vect2 _avgSize;
	private readonly uint _maxItems;
	private int _scrollIndex, _selectedIndex;
	private float _itemTimeout;

	public float PerItemTimeout { get; set; } = 0.255f;
	public ListviewItem SelectedItem => ChildCount > 0
		? (ListviewItem)Children[SelectedIndex] : null;

	public T SelectedItemAs<T>() where T : ListviewItem => (T)SelectedItem;
	public bool AtStart => _scrollIndex == 0 && _selectedIndex == 0;
	public int SelectedIndex => ChildCount > 0
		? _scrollIndex + _selectedIndex : 0;
	public Action<Listview> OnItemSelected;
	public bool AtEnd
	{
		get
		{
			var maxScroll = Math.Max(Children.Count - _maxItems, 0);
			var maxSelected = Children.Count <= _maxItems
				? Children.Count - 1 : (int)_maxItems - 1;

			return _selectedIndex == maxSelected && _scrollIndex == maxScroll;
		}
	}

	public Listview(uint maxItems, params ListviewItem[] items) : base(items)
	{
		if (maxItems == 0)
			throw new ArgumentOutOfRangeException(nameof(maxItems), "maxItems must be greater than zero.");

		_maxItems = maxItems;
		_avgSize = GetAverageSize(items);

		if (_avgSize.X <= 0 || _avgSize.Y <= 0)
			throw new InvalidOperationException("Item size has never been set or item size is zero.");

		Size = new Vect2(_avgSize.X, _avgSize.Y * maxItems);
	}

	private Vect2 GetAverageSize(IEnumerable<Entity> children)
	{
		if (children.IsEmpty())
			return Vect2.Zero;

		var c = children
			.Where(x => x != null && !x.IsExiting && x.IsVisible)
			.ToList();

		return new Vect2(c.Max(x => x.Size.X), c.Max(x => x.Size.Y));
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
		var children = Children.OfType<ListviewItem>().ToList();
		var index = 0;

		foreach (var c in children)
		{
			if (!c.IsVisible)
				continue;

			c.Position = new Vect2(0, offsetY);
			c.Selected = index == _scrollIndex + _selectedIndex;

			if (c.Selected)
				OnItemSelected?.Invoke(this);

			index++;

			offsetY += _avgSize.Y;
		}

		StartRoutine(
			WaitForRenderer(() => Offset = new Vect2(0, _avgSize.Y * _scrollIndex))
		);

		base.OnDirty(state);
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

		var maxScroll = Math.Max(Children.Count - _maxItems, 0);
		var maxSelected = Children.Count <= _maxItems
			? Children.Count - 1
			: (int)_maxItems - 1;

		if (_selectedIndex < maxSelected)
		{
			_selectedIndex++;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
		else if (_scrollIndex < maxScroll)
		{
			_scrollIndex++;
			SetDirtyState(DirtyState.Update);
			_itemTimeout += PerItemTimeout;
		}
	}
}
