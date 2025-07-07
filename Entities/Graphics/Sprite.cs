namespace Snap.Entities.Graphics;

public class Sprite : Entity
{
	private Texture _texture;
	private RenderTarget? _rt;
	private bool _rtChecked;

	public Rect2 Source { get; set; }
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
		Source = source;

		Size = source.Size;
	}

	public Sprite(Texture texture, Spritesheet sheet, string sheetName)
		: this(texture, sheet.GetBounds(sheetName)) { }

	public Sprite(Texture texture) : this(texture, texture.Bounds) { }

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		var offsetX = AlignHelpers.AlignWidth(Size.X, Source.Size.X, HAlign);
		var offsetY = AlignHelpers.AlignHeight(Size.Y, Source.Size.Y, VAlign);

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;
			var final = new Vect2(local.X + offsetX, local.Y + offsetY);

			if (_texture.RepeatedTexture) // don't add repeated textures into the atlas... creates massive amount of source rects
				_rt.DrawBypassAtlas(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
			else
				_rt.Draw(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
		}
		else
		{
			var final = new Vect2(Position.X + offsetX, Position.Y + offsetY);

			if (_texture.RepeatedTexture)
				Renderer.DrawBypassAtlas(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
			else
				Renderer.Draw(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
		}

		base.OnUpdate();
	}
}
