using Snap.Graphics;
using Snap.Logs;
using Snap.Systems;

namespace Snap.Assets.Loaders;

public class Texture : IAsset
{
	private enum TextureState
	{
		Create,
		Load,
		RenderTexture,
		Font // for fnt fonts
	}

	private SFTexture _texture;
	private Vect2 _texSize;
	private Color _texColor;
	private TextureState _state;

	public bool RepeatedTexture
	{
		get => _texture.Repeated;
		set => _texture.Repeated = value;
	}

	public bool SmoothTexture
	{
		get => _texture.Smooth;
		set => _texture.Smooth = value;
	}

	public uint Id { get; }
	public string Tag { get; }
	public bool IsValid { get; private set; }
	public uint Handle => IsValid ? _texture.NativeHandle : 0;
	public int Width => IsValid ? (int)_texture.Size.X : 0;
	public int Height => IsValid ? (int)_texture.Size.Y : 0;
	public Vect2 Size => new Vect2(Width, Height);
	public Rect2 Bounds => new Rect2(Vect2.Zero, Size);

	internal Texture(uint id, string filename)
	{
		Id = id;
		Tag = filename;
		_state = TextureState.Load;
	}

	internal Texture(uint id, byte[] bytes)
	{
		// used for fnt fonts. a must.
		Id = id;
		Tag = $"{bytes.Length:X8}";
		_texture = new SFTexture(bytes);
		_state = TextureState.Font;
		IsValid = true;

		Logger.Instance.Log(LogLevel.Info, $"Created FNT Texture with ID: {Id}, Size: (W{_texture.Size.X}, H{_texture.Size.Y})");
	}

	internal Texture(SFTexture texture)
	{
		Id = AssetManager.Id++;
		_texture = texture;
		_state = TextureState.RenderTexture;
		IsValid = true;

		Logger.Instance.Log(LogLevel.Info, $"Created RT Texture with ID: {Id}, Size: (W{_texture.Size.X}, H{_texture.Size.Y})");
	}

	public Texture(Vect2 size) : this(size, Color.White) { }
	public Texture(Vect2 size, Color color)
	{
		if (size.IsZero)
			throw new Exception();

		Id = AssetManager.Id++;
		Tag = $"{(int)size.X:X8}{(int)size.Y:X8}{color.R:X8}{color.G:X8}{color.B:X8}{color.A:X8}";
		_texSize = size;
		_texColor = color;
		_state = TextureState.Create;
		IsValid = true;

		CreateTexture();
	}

	~Texture()
	{
		if (_state == TextureState.Create || _state == TextureState.RenderTexture)
			Dispose();
	}

	public ulong Load()
	{
		if (IsValid)
			return 0u;

		// Created texture should have been initialized on the constructor 
		// not thru here. If the dev created an texture and puts it on
		// the assets manager, than yes, it will need to create the 
		// texture again (if evicted).
		var result = _state switch
		{
			TextureState.Create => CreateTexture(),
			TextureState.Load => LoadTexture(),

			// Render texture, if stored from
			_ => _texture.Size.X * _texture.Size.Y * 4UL,
		};

		IsValid = true;

		return result;
	}

	public void Unload()
	{
		// never unload render textures even in eviction
		if (_state == TextureState.RenderTexture || !IsValid)
			return;

		Dispose();
	}

	public void Dispose()
	{
		_texture?.Dispose();
		IsValid = false;

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}', State: {_state}.");
	}

	private ulong CreateTexture()
	{
		var sfImage = new SFImage((uint)_texSize.X, (uint)_texSize.Y, _texColor);
		_texture = new SFTexture(sfImage);

		Logger.Instance.Log(LogLevel.Info, $"Created Blank Texture with ID: {Id}, Size: (W{_texture.Size.X}, H{_texture.Size.Y})");

		return _texture.Size.X * _texture.Size.Y * 4;
	}

	private ulong LoadTexture()
	{
		if (!File.Exists(Tag))
			throw new FileNotFoundException();

		var bytes = File.ReadAllBytes(Tag);

		_texture = new SFTexture(bytes);

		return _texture.Size.X * _texture.Size.Y * 4;
	}

	public static implicit operator SFTexture(Texture tex) => tex._texture;
}
