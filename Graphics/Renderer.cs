using System.Runtime.InteropServices;

using Snap.Assets.Fonts;
using Snap.Assets.Loaders;
using Snap.Enums;
using Snap.Logs;
using Snap.Systems;

namespace Snap.Graphics;

public struct DrawCommand
{
	public SFTexture Texture { get; }
	public SFVertex[] Vertex { get; }
	public int Depth { get; }

	public DrawCommand(SFTexture texture, SFVertex[] vertex, int depth)
	{
		Texture = texture;
		Vertex = vertex;
		Depth = depth;
	}
}

public sealed class Renderer
{
	private const int MaxVerticies = 6;

	private SFVertexBuffer _vertexBuffer;
	private SFVertex[] _vertexCache;
	private readonly Dictionary<uint, List<DrawCommand>> _drawCommands = new(16);
	// private DateTime _nextEvictionTime = DateTime.UtcNow.AddMinutes(0.5);
	// private readonly TimeSpan _evictionInterval = TimeSpan.FromMinutes(0.5);
	// private readonly TimeSpan _maxIdle = TimeSpan.FromMinutes(0.5);
	private int _vertexBufferSize, _batches;
	private Camera _camera;

	public int DrawCalls { get; private set; }
	public int Batches { get; private set; }
	public static Renderer Instance { get; private set; }
	// public int AtlasPageUsed => AtlasManager.PagesUsed;
	// public int AtlasTotalPages => AtlasManager.MaxPages;
	// public long AtlasTotalUsedPixels => AtlasManager.TotalUsedPixels;
	// public long AtlasTotalCapacityPixels => AtlasManager.TotalCapacityPixels;
	// public double AtlasTotalFillRatio => AtlasManager.TotalFillRatio;
	// public TextureAtlasManager AtlasManager { get; }

	// public void PrintMetrics()
	// {
	// 	Console.WriteLine($"Pixels Used: {AtlasManager.TotalUsedPixels}/{AtlasManager.TotalCapacityPixels}");
	// 	Console.WriteLine($"Pages: {AtlasManager.PagesUsed}/{AtlasManager.MaxPages}");
	// }

	internal Renderer(int maxDrawCalls = 512)
	{
		Instance ??= this;

		_vertexBufferSize = maxDrawCalls;// * MaxVerticies;
		_vertexBuffer = new((uint)_vertexBufferSize, SFPrimitiveType.Triangles, SFVertexBuffer.UsageSpecifier.Stream);
		// AtlasManager = new TextureAtlasManager(pageSize: 2048, maxPages: 8);
		_vertexCache = new SFVertex[_vertexBufferSize];
	}





	// 1) A single helper that handles both atlas‐draw and direct‐draw,
	//    and calls TouchSlice **only** when we actually hit the atlas.
	private void EnqueueDraw(
		SFTexture texture,
		SFRectI srcIntRect,
		Rect2 dstRect,
		Color color,
		Vect2? origin = null,
		Vect2? scale = null,
		float rotation = 0f,
		TextureEffects effects = TextureEffects.None,
		int depth = 0)
	{
		// Try atlas first (only if it fits)
		if (srcIntRect.Width <= TextureAtlasManager.Instance.PageSize &&
			srcIntRect.Height <= TextureAtlasManager.Instance.PageSize)
		{
			var maybeHandle = TextureAtlasManager.Instance.GetOrCreateSlice(texture, srcIntRect);
			if (maybeHandle.HasValue)
			{
				// build quad from atlas
				var pageTex = TextureAtlasManager.Instance.GetPageTexture(maybeHandle.Value.PageId);
				var sr = maybeHandle.Value.SourceRect;
				var atlasSrc = new Rect2(sr.Left, sr.Top, sr.Width, sr.Height);

				var quad = DrawQuad(
					dstRect, atlasSrc, color,
					origin ?? Vect2.Zero, scale ?? Vect2.One,
					rotation, effects
				);

				EnqueueCommand(pageTex.NativeHandle, pageTex, quad, depth);
				return;
			}
		}

		// Fallback: direct‐draw from the original texture
		var directSrc = new Rect2(
			srcIntRect.Left, srcIntRect.Top,
			srcIntRect.Width, srcIntRect.Height
		);

		var directQuad = DrawQuad(
			dstRect, directSrc, color,
			origin ?? Vect2.Zero, scale ?? Vect2.One,
			rotation, effects
		);

		EnqueueCommand(texture.NativeHandle, texture, directQuad, depth);
	}

	private void EnqueueCommand(
		uint texHandle,
		SFTexture tex,
		SFVertex[] quad,
		int depth
	)
	{
		if (!_drawCommands.TryGetValue(texHandle, out var list))
		{
			list = new List<DrawCommand>();
			_drawCommands[texHandle] = list;
		}
		list.Add(new DrawCommand(tex, quad, depth));
	}




	public void Draw(Texture texture, Rect2 dstRect, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, dstRect, srcRect, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Rect2 rect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, rect, texture.Bounds, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Vect2 position, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, new(position, srcRect.Size), srcRect, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Vect2 position, Rect2 srcRect, Color color, int depth = 0) =>
		EngineDraw(texture, new Rect2(position, srcRect.Size), srcRect, color, depth: depth);
	public void Draw(Texture texture, Vect2 position, Color color, int depth = 0) =>
		EngineDraw(texture, new Rect2(position, texture.Size), texture.Bounds, color, depth: depth);

	public void DrawText(Font font, string text, Vect2 position, Color color, int depth = 0)
		=> EngineDrawText(font, text, position, color, depth);

	public void DrawBypassAtlas(Texture texture, Rect2 rect, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, rect, texture.Bounds, color, depth: depth);
	public void DrawBypassAtlas(Texture texture, Vect2 position, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, new Rect2(position, texture.Size), texture.Bounds, color, depth: depth);
	public void DrawBypassAtlas(Texture texture, Rect2 dst, Rect2 src, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, dst, src, color, depth: depth);

	// private unsafe void EngineDrawText(Font font, string text, Vect2 position, Color color, int depth)
	// {
	// 	// 1) Grab the font's SFML texture once
	// 	var fontTexture = font.GetTexture();
	// 	if (!fontTexture.IsValid)
	// 		return;

	// 	// 2) Track an (x,y) offset from 'position'
	// 	Vect2 offset = Vect2.Zero;

	// 	// 3) Iterate over each character
	// 	fixed (char* ptr = text)
	// 	{
	// 		for (int i = 0, n = text.Length; i < n; i++)
	// 		{
	// 			char c = ptr[i];

	// 			// Support Windows (\r\n) or Linux (\n) line breaks
	// 			if (c == '\r')
	// 				continue;
	// 			if (c == '\n')
	// 			{
	// 				offset.X = 0f;
	// 				offset.Y += font.LineSpacing;
	// 				continue;
	// 			}

	// 			// 4) Lookup the Glyph for this character
	// 			if (!font.Glyphs.TryGetValue(c, out var glyph))
	// 				continue;

	// 			// 5) Compute the glyph's on-screen top-left
	// 			float drawX = (position.X + offset.X) + glyph.XOffset;
	// 			float drawY = (position.Y + offset.Y) + glyph.YOffset;

	// 			// 6) Build the destination rectangle at the glyph's native size
	// 			var dstRect = new Rect2(
	// 				new Vect2(drawX, drawY),
	// 				new Vect2(glyph.Cell.Width, glyph.Cell.Height)
	// 			);

	// 			// 7) Determine atlas vs. direct: only skip atlas if strictly larger than PageSize
	// 			if (glyph.Cell.Width > AtlasManager.Size.X || glyph.Cell.Height > AtlasManager.Size.Y)
	// 			{
	// 				// Direct‐draw path: build a quad using fontTexture + glyph.Cell
	// 				var quad = DrawQuad(
	// 					dstRect,
	// 					new Rect2(glyph.Cell.Left, glyph.Cell.Top, glyph.Cell.Width, glyph.Cell.Height),
	// 					color,
	// 					Vect2.Zero,
	// 					Vect2.One,
	// 					0f,
	// 					TextureEffects.None
	// 				);

	// 				uint texId = fontTexture.Handle;
	// 				if (!_drawCommands.TryGetValue(texId, out var list))
	// 				{
	// 					list = new List<DrawCommand>();
	// 					_drawCommands[texId] = list;
	// 				}
	// 				list.Add(new DrawCommand(fontTexture, quad, depth));
	// 			}
	// 			else
	// 			{
	// 				// Atlas path: pack (or retrieve) this glyph’s cell into the atlas
	// 				var srcIntRect = new SFRectI(
	// 					(int)glyph.Cell.Left,
	// 					(int)glyph.Cell.Top,
	// 					(int)glyph.Cell.Width,
	// 					(int)glyph.Cell.Height
	// 				);

	// 				AtlasHandle? maybeHandle = AtlasManager.GetOrCreateSlice(fontTexture, srcIntRect);
	// 				if (maybeHandle.HasValue)
	// 				{
	// 					var handle = maybeHandle.Value;
	// 					var pageTex = AtlasManager.GetPageTexture(handle.PageId);

	// 					// Build the Rect2 for the atlas sub-rect
	// 					var atlasSrc = new Rect2(
	// 						handle.SourceRect.Left,
	// 						handle.SourceRect.Top,
	// 						handle.SourceRect.Width,
	// 						handle.SourceRect.Height
	// 					);

	// 					var quad = DrawQuad(
	// 						dstRect,
	// 						atlasSrc,
	// 						color,
	// 						Vect2.Zero,
	// 						Vect2.One,
	// 						0f,
	// 						TextureEffects.None
	// 					);

	// 					uint texId = pageTex.NativeHandle;
	// 					if (!_drawCommands.TryGetValue(texId, out var list))
	// 					{
	// 						list = new List<DrawCommand>();
	// 						_drawCommands[texId] = list;
	// 					}
	// 					list.Add(new DrawCommand(pageTex, quad, depth));
	// 					AtlasManager.TouchSlice(fontTexture, atlasSrc);
	// 				}
	// 				else
	// 				{
	// 					var rect = new Rect2(glyph.Cell.Left, glyph.Cell.Top, glyph.Cell.Width, glyph.Cell.Height);
	// 					// Atlas is full (or insertion failed) → fallback to direct‐draw
	// 					var quad = DrawQuad(
	// 						dstRect,
	// 						rect,
	// 						color,
	// 						Vect2.Zero,
	// 						Vect2.One,
	// 						0f,
	// 						TextureEffects.None
	// 					);

	// 					uint texId = fontTexture.Handle;
	// 					if (!_drawCommands.TryGetValue(texId, out var list))
	// 					{
	// 						list = new List<DrawCommand>();
	// 						_drawCommands[texId] = list;
	// 					}
	// 					list.Add(new DrawCommand(fontTexture, quad, depth));
	// 					AtlasManager.TouchSlice(fontTexture, rect);
	// 				}
	// 			}

	// 			// 8) Advance cursor X by glyph.Advance
	// 			offset.X += glyph.Advance;
	// 		}
	// 	}
	// }
	private unsafe void EngineDrawText(Font font, string text, Vect2 position, Color color, int depth)
	{
		var fontTex = font.GetTexture();
		if (fontTex.IsInvalid) return;

		Vect2 offset = Vect2.Zero;
		fixed (char* p = text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				char c = p[i];
				if (c == '\r') continue;
				if (c == '\n')
				{
					offset.X = 0;
					offset.Y += font.LineSpacing;
					continue;
				}

				if (!font.Glyphs.TryGetValue(c, out var g)) continue;

				// compute on‐screen dst rect
				var dst = new Rect2(
					new Vect2(position.X + offset.X + g.XOffset,
							  position.Y + offset.Y + g.YOffset),
					new Vect2(g.Cell.Width, g.Cell.Height)
				);

				// source in font texture
				var srcInt = new SFRectI(
					(int)g.Cell.Left,
					(int)g.Cell.Top,
					(int)g.Cell.Width,
					(int)g.Cell.Height
				);

				EnqueueDraw(fontTex, srcInt, dst, color, depth: depth);

				offset.X += g.Advance;
			}
		}
	}

	private void EngineDrawBypassAtlas(
	Texture texture,
	Rect2 dstRect,
	Rect2 srcRect,
	Color color,
	Vect2? origin = null,
	Vect2? scale = null,
	float rotation = 0f,
	TextureEffects effects = TextureEffects.None,
	int depth = 0)
	{
		// if (!texture.IsValid)
		// 	texture.Load();

		var quad = DrawQuad(
				dstRect,
				srcRect,
				color,
				origin ?? Vect2.Zero,
				scale ?? Vect2.One,
				rotation,
				effects);

		uint textureId = texture.Handle;
		if (!_drawCommands.TryGetValue(textureId, out var list))
		{
			list = new List<DrawCommand>();
			_drawCommands[textureId] = list;
		}
		list.Add(new DrawCommand(texture, quad, depth));
	}

	private void EngineDraw(
	Texture texture,
	Rect2 dstRect,
	Rect2 srcRect,
	Color color,
	Vect2? origin = null,
	Vect2? scale = null,
	float rotation = 0f,
	TextureEffects effects = TextureEffects.None,
	int depth = 0)
	{
		if (!_camera.CullBounds.Intersects(dstRect))
			return;
		if (!texture.IsValid)
			texture.Load();

		// convert float‐Rect2 → IntRect
		var srcInt = new SFRectI(
			(int)srcRect.Left,
			(int)srcRect.Top,
			(int)srcRect.Width,
			(int)srcRect.Height
		);

		EnqueueDraw(texture, srcInt, dstRect, color, origin, scale, rotation, effects, depth);
	}


	// private void EngineDraw(
	// Texture texture,
	// Rect2 dstRect,
	// Rect2 srcRect,
	// Color color,
	// Vect2? origin = null,
	// Vect2? scale = null,
	// float rotation = 0f,
	// TextureEffects effects = TextureEffects.None,
	// int depth = 0)
	// {
	// 	if (!_camera.CullBounds.Intersects(dstRect))
	// 		return;
	// 	// if (!texture.IsValid)
	// 	// 	texture.Load();

	// 	// If the source‐rect is bigger than our atlas pages, draw directly:
	// 	if (srcRect.Size.X > AtlasManager.PageSize || srcRect.Size.Y > AtlasManager.PageSize)
	// 	{
	// 		var quad = DrawQuad(
	// 			dstRect,
	// 			srcRect,
	// 			color,
	// 			origin ?? Vect2.Zero,
	// 			scale ?? Vect2.One,
	// 			rotation,
	// 			effects);

	// 		uint textureId = texture.Handle;
	// 		if (!_drawCommands.TryGetValue(textureId, out var list))
	// 		{
	// 			list = new List<DrawCommand>();
	// 			_drawCommands[textureId] = list;
	// 		}
	// 		list.Add(new DrawCommand(texture, quad, depth));
	// 	}
	// 	else
	// 	{
	// 		// Try to pack (or retrieve) this sub‐rect into our atlas:
	// 		AtlasHandle? maybeHandle = AtlasManager.GetOrCreateSlice(texture, srcRect);

	// 		if (!maybeHandle.HasValue)
	// 		{
	// 			// If atlas is full or failed, fall back to drawing directly:
	// 			var quad = DrawQuad(
	// 				dstRect,
	// 				srcRect,
	// 				color,
	// 				origin ?? Vect2.Zero,
	// 				scale ?? Vect2.One,
	// 				rotation,
	// 				effects);

	// 			uint textureId = texture.Handle;
	// 			if (!_drawCommands.TryGetValue(textureId, out var list))
	// 			{
	// 				list = new List<DrawCommand>();
	// 				_drawCommands[textureId] = list;
	// 			}
	// 			list.Add(new DrawCommand(texture, quad, depth));
	// 		}
	// 		else
	// 		{
	// 			// We successfully got an atlas handle:
	// 			var handle = maybeHandle.Value;
	// 			SFTexture pageTexture = AtlasManager.GetPageTexture(handle.PageId);

	// 			// Build the Rect2 for the atlas sub‐rect:
	// 			var atlasSrc = new Rect2(
	// 				handle.SourceRect.Left,
	// 				handle.SourceRect.Top,
	// 				handle.SourceRect.Width,
	// 				handle.SourceRect.Height);

	// 			var quad = DrawQuad(
	// 				dstRect,
	// 				atlasSrc,
	// 				color,
	// 				origin ?? Vect2.Zero,
	// 				scale ?? Vect2.One,
	// 				rotation,
	// 				effects);

	// 			uint textureId = pageTexture.NativeHandle;
	// 			if (!_drawCommands.TryGetValue(textureId, out var list))
	// 			{
	// 				list = new List<DrawCommand>();
	// 				_drawCommands[textureId] = list;
	// 			}
	// 			list.Add(new DrawCommand(pageTexture, quad, depth));

	// 			AtlasManager.TouchSlice(texture, srcRect);
	// 		}
	// 	}
	// }

	internal SFVertex[] DrawQuad(
		Rect2 dstRect,
		Rect2 srcRect,
		Color color,
		Vect2 origin,
		Vect2 scale,
		float rotation,
		TextureEffects effects)
	{

		var result = new SFVertex[MaxVerticies];

		// Compute a pivot that accounts for both origin and scale exactly once:
		float pivotX = origin.X * dstRect.Width * scale.X;
		float pivotY = origin.Y * dstRect.Height * scale.Y;

		// Build “local” corner positions already multiplied by scale:
		var localPos = new SFVectF[4];
		localPos[0] = new SFVectF(-pivotX, -pivotY);
		localPos[1] = new SFVectF(dstRect.Width * scale.X - pivotX, -pivotY);
		localPos[2] = new SFVectF(dstRect.Width * scale.X - pivotX, dstRect.Height * scale.Y - pivotY);
		localPos[3] = new SFVectF(-pivotX, dstRect.Height * scale.Y - pivotY);

		float cos = MathF.Cos(rotation);
		float sin = MathF.Sin(rotation);

		// Rotate each corner and then translate by (dstRect.X, dstRect.Y) + pivot
		for (int i = 0; i < localPos.Length; i++)
		{
			float x = localPos[i].X;
			float y = localPos[i].Y;

			localPos[i].X = cos * x - sin * y + dstRect.X + pivotX;
			localPos[i].Y = sin * x + cos * y + dstRect.Y + pivotY;
		}

		// Texture coordinates (UVs)
		float u1 = srcRect.Left;
		float v1 = srcRect.Top;
		float u2 = srcRect.Right;
		float v2 = srcRect.Bottom;

		if (effects.HasFlag(TextureEffects.FlipHorizontal))
		{
			(u1, u2) = (u2, u1);
		}
		if (effects.HasFlag(TextureEffects.FlipVertical))
		{
			(v1, v2) = (v2, v1);
		}

		// Build two triangles (6 vertices)
		result[0] = new SFVertex(localPos[0], color, new SFVectF(u1, v1));
		result[1] = new SFVertex(localPos[1], color, new SFVectF(u2, v1));
		result[2] = new SFVertex(localPos[3], color, new SFVectF(u1, v2));
		result[3] = new SFVertex(localPos[1], color, new SFVectF(u2, v1));
		result[4] = new SFVertex(localPos[2], color, new SFVectF(u2, v2));
		result[5] = new SFVertex(localPos[3], color, new SFVectF(u1, v2));

		return result;
	}



	internal void Begin(Camera camera)
	{
		Engine.Instance.ToRenderer.SetView(camera.ToEngine);

		_camera = camera;

		DrawCalls = (int)_vertexBuffer.VertexCount;
		Batches = _batches;

		_drawCommands.Clear();
		_batches = 0;
	}



	internal void End()
	{
		// var temp = _vertexCache;
		var index = 0;
		SFTexture currentTexture = null;

		// Iterate each texture‐bucket in your original order
		foreach (var kvp in _drawCommands)
		{
			var commands = kvp.Value;
			commands.Sort((a, b) => a.Depth.CompareTo(b.Depth));

			foreach (ref readonly var cmd in CollectionsMarshal.AsSpan(commands))
			{
				bool willOverflow = index + cmd.Vertex.Length > _vertexBufferSize;
				bool textureChanged = currentTexture != null && currentTexture != cmd.Texture;

				// If the texture changed, flush whatever we’ve collected so far
				// if (currentTexture != null && currentTexture != cmd.Texture)
				if(willOverflow || textureChanged)
				{
					// Flush(index, temp, currentTexture);
					if(index > 0 && currentTexture != null)
						Flush(index, _vertexCache, currentTexture);
					index = 0;

					if(willOverflow)
						EnsureVertexBufferCapacity(Math.Max(_vertexBufferSize * 2, cmd.Vertex.Length));
				}

				// Copy this sprite’s verts into the cache
				// for (int i = 0; i < cmd.Vertex.Length; i++)
				// {
				// 	_vertexCache[index++] = cmd.Vertex[i];

				// 	// if (index >= _vertexBufferSize)
				// 	// {
				// 	// 	EnsureVertexBufferCapacity(index + MaxVerticies);
				// 	// 	// flush the old batch before resizing
				// 	// 	Flush(index, temp, currentTexture);
				// 	// 	index = 0;
				// 	// 	temp = _vertexCache;
				// 	// }
				// 	// temp[index++] = cmd.Vertex[i];
				// }
				var src = cmd.Vertex.AsSpan();
				var dst = _vertexCache.AsSpan(index, src.Length);
				src.CopyTo(dst);
				index += src.Length;

				// Now we’re “in” this texture
				currentTexture = cmd.Texture;
			}
		}

		// Flush any remaining verts for the last texture
		if (index > 0 && currentTexture != null)
		{
			// Flush(index, temp, currentTexture);
			Flush(index, _vertexCache, currentTexture);
		}

		// Clear for next frame
		_drawCommands.Clear();
	}

	private void EnsureVertexBufferCapacity(int neededSize)
	{
		if (neededSize <= _vertexBufferSize)
			return;

		// double the size until big enough:
		int newSize = _vertexBufferSize;
		while (newSize < neededSize)
			newSize *= 2;

		Logger.Instance.Log(LogLevel.Info, $"[Renderer]: Resizing vertex buffer array to {newSize}");

		_vertexBuffer.Dispose();
		_vertexBuffer = new SFVertexBuffer((uint)newSize, SFPrimitiveType.Triangles, SFVertexBuffer.UsageSpecifier.Stream);

		Array.Resize(ref _vertexCache, newSize);

		_vertexBufferSize = newSize;
	}

	internal void Flush(int vertexCount, SFVertex[] vertices, SFTexture texture)
	{
		if (vertexCount == 0 || texture == null)
			return;

		// ZERO OUT any leftover verts so they don't draw
		var totalVerts = vertices.Length;
		if (vertexCount < totalVerts)
			Array.Clear(vertices, vertexCount, totalVerts - vertexCount);

		_vertexBuffer.Update(vertices);

		Engine.Instance.ToRenderer.Draw(_vertexBuffer, new SFRenderStates
		{
			Texture = texture,
			Transform = SFTransform.Identity,
			BlendMode = SFBlendMode.Alpha,
		});

		_batches++;
	}
}

