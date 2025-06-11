using Snap.Assets.Loaders;
using Snap.Graphics;
using Snap.Logs;
using Snap.Systems;

namespace Snap.Assets.Fonts;

public class Font : IAsset
{
	internal SFTexture Texture { get; set; }

	public Dictionary<char, Glyph> Glyphs { get; protected set; } = new();
	public Dictionary<(uint first, uint second), int> KerningLookup { get; protected set; } = new();
	public uint Id { get; protected set; }
	public string Tag { get; protected set; }
	public bool IsValid { get; protected set; }
	public ulong Length { get; protected set; }
	public virtual float Spacing { get; protected set; }
	public uint Handle => IsValid ? Texture.NativeHandle : 0;

	public virtual float GetSpacing(char c)
	{
		var g = Glyphs[c];

		return g.Advance + Spacing;
	}
	public virtual float LineSpacing { get; }

	public Font(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}

	public virtual ulong Load() { return Length; }

	public virtual void Unload()
	{
		if (!IsValid)
			return;

		Dispose();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset {Id}, of {GetType().Name}");

		Length = 0ul;
	}

	public virtual void Dispose()
	{
		Texture?.Dispose();
		IsValid = false;
	}

	public Vect2 Measure(string text)
	{
		var result = Vect2.Zero;
		float offsetX = 0f, offsetY = LineSpacing;

		for (int i = 0; i < text.Length; i++)
		{
			var c = text[i];

			if (!Glyphs.TryGetValue(c, out var glyph))
				continue;
			if (c == '\r')
				continue;
			if (c == '\n')
			{
				offsetX = 0f;
				offsetY += LineSpacing;
				continue;
			}

			offsetX += glyph.Advance;

			if (result.X < offsetX)
				result.X = offsetX;
			if (result.Y < offsetY)
				result.Y = offsetY;
		}

		return result;
	}

	public virtual SFTexture GetTexture() { return default; }


}
