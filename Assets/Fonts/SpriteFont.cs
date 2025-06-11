using Snap.Assets.Loaders;
using Snap.Systems;

namespace Snap.Assets.Fonts;

public sealed class SpriteFont : Font
{
	private readonly bool _smoothing;
	private readonly float _spacing;
	private readonly float _lineSpacing;
	private readonly string _charList;
	private float _finalLineSpacing;

	public override float LineSpacing => _finalLineSpacing + _lineSpacing;
	public override float Spacing => _spacing;

	internal SpriteFont(uint id, string filename, float spacing, float lineSpacing,
		bool smoothing = false, string charList = null) : base(id, filename)
	{
		_smoothing = smoothing;
		_spacing = spacing;
		_lineSpacing = lineSpacing;
		_charList = charList;
	}
	// ~SpriteFont() => Dispose();

	public override void Dispose()
	{
		if (!IsValid)
			return;

		Glyphs.Clear();

		base.Dispose();
	}

	public override void Unload()
	{
		// Unload not used here. Dont remove but keep it 
		// blank. Everything is done thru the base.Unload();

		base.Unload();
	}

	public override ulong Load()
	{
		if (IsValid)
			return 0u;

		if (!File.Exists(Tag))
			throw new Exception();

		var bytes = File.ReadAllBytes(Tag);
		var seq = _charList.IsEmpty()
			? " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOP" +
				"QRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}"
			: _charList;

		Texture = new SFTexture(File.ReadAllBytes(Tag))
		{
			Smooth = _smoothing
		};

		Glyphs = LoadBorderCells(Tag, seq, _spacing);

		_finalLineSpacing = Glyphs.Max(x => x.Value.Cell.Height);

		Length = (ulong)Texture.Size.X * (ulong)Texture.Size.Y * 4UL;

		IsValid = true;

		return Length;
	}

	public override SFTexture GetTexture()
	{
		if (Texture == null || Texture.IsInvalid)
			throw new Exception();

		// if (Texture == null || Texture.IsInvalid)
		// {
		// 	Texture?.Dispose();

		// 	Texture = new SFTexture(File.ReadAllBytes(Tag))
		// 	{
		// 		Smooth = _smoothing
		// 	};
		// }

		return Texture;
	}

	private Dictionary<char, Glyph> LoadBorderCells(string imagePath, string asciiSequence, float spacing = 0f)
	{
		SFImage img = new SFImage(imagePath);
		uint w = img.Size.X, h = img.Size.Y;

		SFColor topLeft = img.GetPixel(0, 0);

		bool isFullyTransparent = topLeft.A == 0;

		if (isFullyTransparent)
		{
			throw new InvalidOperationException(
				"No solid border detected. Reach cell must be outlinted by a unique solid color"
			);
		}

		SFColor borderColor = topLeft;

		bool[,] visited = new bool[w, h];
		var componetRects = new List<SFRectI>();

		bool isMagenta(uint x, uint y)
		{
			var px = img.GetPixel(x, y);
			return px.R == topLeft.R && px.G == topLeft.G && px.B == topLeft.B && px.A == topLeft.A;
		}

		for (uint y = 0; y < h; y++)
		{
			for (uint x = 0; x < w; x++)
			{
				if (visited[x, y] || isMagenta(x, y))
					continue;

				var queue = new Queue<(uint x, uint y)>();
				queue.Enqueue((x, y));
				visited[x, y] = true;

				uint minX = x, maxX = x, minY = y, maxY = y;

				while (queue.Count > 0)
				{
					var (cx, cy) = queue.Dequeue();

					if (cx < minX) minX = cx;
					if (cx > maxX) maxX = cx;
					if (cy < minY) minY = cy;
					if (cy > maxY) maxY = cy;

					foreach (var (dx, dy) in new (int, int)[]
						{(1,0),(-1,0), (0,1), (0,-1)})
					{
						int nx = (int)cx + dx, ny = (int)cy + dy;
						if (nx < 0 || ny < 0 || nx >= w || ny >= h)
							continue;
						uint ux = (uint)nx, uy = (uint)ny;
						if (visited[ux, uy]) continue;
						if (isMagenta(ux, uy)) continue;

						visited[ux, uy] = true;
						queue.Enqueue((ux, uy));
					}
				}

				componetRects.Add(new SFRectI((int)minX, (int)minY, (int)(maxX - minX + 1), (int)(maxY - minY + 1)));
			}
		}

		int verticalTolerance = 2;
		componetRects.Sort((a, b) =>
		{
			if (a.Top + verticalTolerance < b.Top) return -1;
			if (a.Top > b.Top + verticalTolerance) return 1;
			return a.Left.CompareTo(b.Left);
		});

		if (componetRects.Count < asciiSequence.Length - 1)
		{
			throw new InvalidOperationException(
				$"LoadTransparentAtlas found {componetRects.Count} cells but expected {asciiSequence.Length}" +
				$"Check that (a) boundground is magenta everywhere out of cells, (B) each border is solid border Color. (C) you passed incorrect asciiSequence"
			);
		}

		var glyphCells = new Dictionary<char, Glyph>(asciiSequence.Length);

		for (int i = 0; i < asciiSequence.Length; i++)
		{
			var outer = componetRects[i];
			int x0 = outer.Left, x1 = outer.Left + outer.Width - 1;
			int y0 = outer.Top, y1 = outer.Top + outer.Height - 1;

			while (x0 <= x1)
			{
				bool columnAllBorder = true;
				for (int yy = y0; yy <= y1; yy++)
				{
					var px = img.GetPixel((uint)x0, (uint)yy);
					if (!(px.R == borderColor.R && px.G == borderColor.G && px.B == borderColor.B && px.A == borderColor.A))
					{
						columnAllBorder = false;
						break;
					}
				}
				if (!columnAllBorder) break;
				x0++;
			}

			while (x1 >= x0)
			{
				bool columnAllBorder = true;
				for (int yy = y0; yy <= y1; yy++)
				{
					var px = img.GetPixel((uint)x1, (uint)yy);
					if (!(px.R == borderColor.R && px.G == borderColor.G && px.B == borderColor.B && px.A == borderColor.A))
					{
						columnAllBorder = false;
						break;
					}
				}
				if (!columnAllBorder) break;
				x1--;
			}

			while (y0 >= y1)
			{
				bool columnAllBorder = true;
				for (int xx = x0; xx < x1; xx++)
				{
					var px = img.GetPixel((uint)xx, (uint)y0);
					if (!(px.R == borderColor.R && px.G == borderColor.G && px.B == borderColor.B && px.A == borderColor.A))
					{
						columnAllBorder = false;
						break;
					}
				}
				if (!columnAllBorder) break;
				y0++;
			}

			while (y1 >= y0)
			{
				bool columnAllBorder = true;
				for (int xx = x0; xx < x1; xx++)
				{
					var px = img.GetPixel((uint)xx, (uint)y1);
					if (!(px.R == borderColor.R && px.G == borderColor.G && px.B == borderColor.B && px.A == borderColor.A))
					{
						columnAllBorder = false;
						break;
					}
				}
				if (!columnAllBorder) break;
				y1--;
			}

			int innerW = x1 - x0 + 1;
			int innerH = y1 - y0 + 1;

			if (innerW <= 0 || innerH <= 0)
				throw new Exception();

			var innerRect = new SFRectI(x0, y0, innerW, innerH);
			int advance = innerW + (int)spacing;

			glyphCells[asciiSequence[i]] = new Glyph
			{
				Character = asciiSequence[i],
				Cell = new Rect2(innerRect),
				XOffset = 0,
				YOffset = 0,
				Advance = innerW + (int)spacing,
				Page = 0
			};
		}

		return glyphCells;
	}
}
