using System.Runtime.Intrinsics.X86;

using Snap.Assets.Loaders;
using Snap.Assets.Spritesheets;
using Snap.Enums;
using Snap.Graphics;
using Snap.Helpers;
using Snap.Systems;

namespace Snap.Entities.Graphics;

public class Sprite : Entity
{
	private Texture _texture;
	private Rect2 _source;
	private RenderTarget? _rt;
	private bool _rtChecked;

	public Color Color { get; set; } = Color.White;
	public Vect2 Origin { get; set; }
	public TextureEffects Effects { get; set; }
	public Vect2 Scale { get; set; } = Vect2.One;
	public float Rotation { get; set; } = 0f;
	public HAlign HAlign { get; set; }
	public VAlign VAlign { get; set; }

	public Sprite(Texture texture, Rect2 source)
	{
		_texture = texture;
		_source = source;

		Size = source.Size;
	}

	public Sprite(Texture texture, Spritesheet sheet, string sheetName)
		: this(texture, sheet.GetBounds(sheetName)) { }

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (Color.A == 0 || !IsVisible)
			return;

		var offsetX = AlignHelpers.AlignWidth(Size.X, _source.Size.X, HAlign);
		var offsetY = AlignHelpers.AlignHeight(Size.Y, _source.Size.Y, VAlign);

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;
			var final = new Vect2(local.X + offsetX, local.Y + offsetY);

			_rt.Draw(_texture, final, _source, Color, Origin, Scale, Rotation, Effects, Layer);
		}
		else
		{
			var final = new Vect2(Position.X + offsetX, Position.Y + offsetY);

			Renderer.Draw(_texture, final, _source, Color, Origin, Scale, Rotation, Effects, Layer);
		}

		base.OnUpdate();
	}
}
