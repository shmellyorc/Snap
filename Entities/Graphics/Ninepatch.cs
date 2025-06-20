using System.Dynamic;
using System.Security.Cryptography.X509Certificates;

using Microsoft.VisualBasic;

using Snap.Assets.Loaders;
using Snap.Assets.Spritesheets;
using Snap.Entities.Panels;
using Snap.Systems;

namespace Snap.Entities.Graphics;

public sealed class Ninepatch : Entity
{
	[Flags]
	private enum NinePatchDirtyFlags
	{
		None,
		Dest = 1,
		Source = 2,
	}

	private Texture _texture;
	private readonly Rect2 _source;
	private int _left, _right, _top, _bottom;
	private readonly Rect2[] _src = new Rect2[9], _dst = new Rect2[9];
	private NinePatchDirtyFlags _dirtyFlags;


	public Color Color { get; set; } = Color.White;
	public new Vect2 Position
	{
		get => base.Position;
		set
		{
			if (base.Position == value)
				return;
			base.Position = value;

			_dirtyFlags |= NinePatchDirtyFlags.Dest;
		}
	}

	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;

			_dirtyFlags |= NinePatchDirtyFlags.Dest;
		}
	}

	public Ninepatch(Texture texture, Rect2 source, Rect2 corners)
	{
		_texture = texture;
		_source = source;

		// after storing _srcRegion = source;
		_left = (int)corners.Left;
		_top = (int)corners.Top;
		_right = (int)corners.Right;
		_bottom = (int)corners.Bottom;

		_dirtyFlags = NinePatchDirtyFlags.Dest | NinePatchDirtyFlags.Source;
	}

	public Ninepatch(Texture texture, Spritesheet sheet, string patchName)
		: this(texture, sheet.GetBounds(patchName), sheet.GetPatch(patchName)) { }

	private RenderTarget? _rt;
	private bool _rtChecked;

	Rect2 _oldBounds;

	protected override void OnEnter()
	{
		_oldBounds = Bounds;

		CreatePatches(_source, _src);
		CreatePatches(Bounds, _dst);

		base.OnEnter();
	}

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (_oldBounds != Bounds)
		{
			_dirtyFlags |= NinePatchDirtyFlags.Dest;
			_oldBounds = Bounds;
		}

		if (_dirtyFlags != NinePatchDirtyFlags.None)
		{
			if (_dirtyFlags.HasFlag(NinePatchDirtyFlags.Source))
				CreatePatches(_source, _src);
			if (_dirtyFlags.HasFlag(NinePatchDirtyFlags.Dest))
				CreatePatches(Bounds, _dst);

			_dirtyFlags = NinePatchDirtyFlags.None;
		}

		if (Color.A <= 0 || !IsVisible)
			return;

		if (_rt != null)
		{
			for (int i = 0; i < 9; i++)
				_rt.DrawBypassAtlas(_texture, _dst[i], _src[i], Color, Layer);
		}
		else
		{
			for (int i = 0; i < 9; i++)
				Renderer.DrawBypassAtlas(_texture, _dst[i], _src[i], Color, Layer);
		}

		base.OnUpdate();
	}



	private void CreatePatches(Rect2 sourceRectangle, Rect2[] patchCache)
	{
		float x = sourceRectangle.X;
		float y = sourceRectangle.Y;
		float w = sourceRectangle.Width;
		float h = sourceRectangle.Height;

		float middleWidth = w - _left - _right;
		float middleHeight = h - _top - _bottom;

		float[] xs = { x, x + _left, x + w - _right };
		float[] ys = { y, y + _top, y + h - _bottom };
		float[] widths = { _left, middleWidth, _right };
		float[] heights = { _top, middleHeight, _bottom };

		for (int row = 0, i = 0; row < 3; row++)
		{
			for (int col = 0; col < 3; col++, i++)
				patchCache[i] = new Rect2(xs[col], ys[row], widths[col], heights[row]);
		}
	}
}
