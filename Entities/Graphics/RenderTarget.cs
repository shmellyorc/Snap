
using System.Collections;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

using Microsoft.Win32;

using Snap.Assets.Fonts;
using Snap.Assets.Loaders;
using Snap.Coroutines.Routines.Conditionals;
using Snap.Coroutines.Routines.Time;
using Snap.Entities.Panels;
using Snap.Enums;
using Snap.Graphics;
using Snap.Logs;
using Snap.Systems;

namespace Snap.Entities.Graphics;

public class RenderTarget : Panel
{
	private const int MaxDrawCalls = 256;
	private const int MaxVerticies = 6;

	private readonly Dictionary<uint, List<DrawCommand>> _drawCommands = new(8);
	private int _vertexBufferSize, _batches;
	private SFRenderTexture _rendTexture;
	private SFVertexBuffer _vertexBuffer;
	private SFVertex[] _vertexCache;
	private Texture _texture;
	public bool IsRendering;
	private SFView _view;

	public Color Color { get; set; } = Color.White;

	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			if (value.X <= 0 || value.Y <= 0)
				return;

			base.Size = value;

			_rendTexture?.Dispose();
			_texture?.Dispose();
			_view?.Dispose();

			_rendTexture = new SFRenderTexture((uint)base.Size.X, (uint)base.Size.Y);
			_texture = new(_rendTexture.Texture);
			_view = new SFView(new SFRectF(0, 0, Size.X, Size.Y));
			_rendTexture.SetView(_view);
		}
	}


	public RenderTarget(params Entity[] entities) : base(entities) { }

	protected override void OnEnter()
	{
		if (Size.X <= 0 || Size.Y <= 0)
			throw new Exception();
		if (this.HasAncestorOfType<RenderTarget>())
			throw new Exception(); // cannot have nested render targets
		if (_rendTexture != null && (_rendTexture.Size.X != Size.X || _rendTexture.Size.Y != Size.Y))
		{
			_rendTexture?.Dispose();
			_texture.Dispose();

			_rendTexture = new SFRenderTexture((uint)Size.X, (uint)Size.Y);
			_texture = new Texture(_rendTexture.Texture);
		}

		_vertexBufferSize = MaxDrawCalls;
		_vertexBuffer = new((uint)_vertexBufferSize, SFPrimitiveType.Triangles, SFVertexBuffer.UsageSpecifier.Dynamic);
		_vertexCache = new SFVertex[_vertexBufferSize];
		_view = new SFView(new SFRectF(0, 0, Size.X, Size.Y));
		_rendTexture.SetView(_view);

		base.OnEnter();
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
		list.Add(new DrawCommand(tex, quad, depth, _seqCounter++));
		// _allCommands.Add(cmd);
	}


	internal void RenderAll()
	{
		var index = 0;
		SFTexture currentTexture = null;
		// var all = new List<DrawCommand>(_drawCommands.Sum(kv => kv.Value.Count));
		// foreach (var bucket in _drawCommands.Values)
		// 	all.AddRange(bucket);
		var allCommands = _drawCommands.Values
			.SelectMany(list => list)
			.OrderBy(cmd => cmd.Depth)
			.ThenBy(cmd => cmd.Sequence);

		// all.Sort((a, b) => a.Depth.CompareTo(b.Depth));
		// allCommands.Sort((a, b) =>
		// {
		// 	int d = a.Depth.CompareTo(b.Depth);
		// 	return d != 0 ? d : a.Sequence.CompareTo(b.Sequence);
		// });

		// 2) Iterate in perfect depth/sequence order,
		//    but still batch whenever the texture doesn't change
		foreach (ref readonly var cmd in CollectionsMarshal.AsSpan(allCommands.ToList()))
		{
			bool willOverflow = index + cmd.Vertex.Length > _vertexBufferSize;
			bool textureChanged = currentTexture != null && currentTexture != cmd.Texture;

			if (willOverflow || textureChanged)
			{
				if (index > 0 && currentTexture != null)
					Flush(index, _vertexCache, currentTexture);
				index = 0;

				if (willOverflow)
					// EnsureVertexBufferCapacity(Math.Max(_vertexBufferSize * 2, cmd.Vertex.Length));
					EnsureVertexBufferCapacity(index + cmd.Vertex.Length);
			}

			// copy verts into the cache
			var src = cmd.Vertex.AsSpan();
			var dst = _vertexCache.AsSpan(index, src.Length);
			src.CopyTo(dst);
			index += src.Length;

			currentTexture = cmd.Texture;
		}

		// 3) Final flush
		if (index > 0 && currentTexture != null)
			Flush(index, _vertexCache, currentTexture);

		// int index = 0;
		// SFTexture currentTexture = null;
		// foreach (var cmd in allCommands)
		// {
		// 	bool willOverflow = index + cmd.Vertex.Length > _vertexBufferSize;
		// 	bool textureChanged = currentTexture != null && currentTexture != cmd.Texture;

		// 	if (willOverflow || textureChanged)
		// 	{
		// 		if (index > 0 && currentTexture != null)
		// 			Flush(index, _vertexCache, currentTexture);
		// 		index = 0;

		// 		if (willOverflow)
		// 			EnsureVertexBufferCapacity(Math.Max(_vertexBufferSize * 2, cmd.Vertex.Length));
		// 	}

		// 	var src = cmd.Vertex.AsSpan();
		// 	var dst = _vertexCache.AsSpan(index, src.Length);
		// 	src.CopyTo(dst);
		// 	index += src.Length;

		// 	currentTexture = cmd.Texture;
		// }

		// if (index > 0 && currentTexture != null)
		// 	Flush(index, _vertexCache, currentTexture);

		_drawCommands.Clear();
		_seqCounter = 0;
		// _batches = 0;
	}

	private void EnsureVertexBufferCapacity(int neededSize)
	{
		if (neededSize <= _vertexBufferSize)
			return;

		// double the size until big enough:
		int newSize = _vertexBufferSize;
		while (newSize < neededSize)
			newSize *= 2;

		Logger.Instance.Log(LogLevel.Info, $"Resizing Render Target Vertex buffer to {newSize}");

		_vertexBuffer.Dispose();
		_vertexBuffer = new SFVertexBuffer((uint)newSize, SFPrimitiveType.Triangles, SFVertexBuffer.UsageSpecifier.Dynamic);

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

		_rendTexture.Draw(_vertexBuffer, new SFRenderStates
		{
			Texture = texture,
			Transform = SFTransform.Identity,
			BlendMode = SFBlendMode.Alpha,
		});

		_batches++;
	}

	private Vect2 _offset;
	public Vect2 Offset
	{
		get => _offset;
		set
		{
			if (_offset == value)
				return;
			_offset = value;

			if (_view == null || _view.IsInvalid)
				throw new Exception();

			_view.Center = new SFVectF(
				Size.X / 2 + _offset.X, Size.Y / 2 + _offset.Y);
		}
	}

	public void Pan(Vect2 delta) => _view.Center += delta;
	public void Zoom(float factor) => _view.Zoom(factor);

	protected override void OnUpdate()
	{
		IsRendering = true;

		_rendTexture.Clear(SFColor.Transparent);
		_rendTexture.SetView(_view);

		RenderAll();

		_rendTexture.Display();

		Renderer.DrawBypassAtlas(_texture, Position, Color, Layer);

		IsRendering = false;

		// if (EngineSettings.Instance.DebugDraw)
		// 	BE.Renderer.DrawRectangleOutline(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, 1f, BoxColor.AllShades.Teal);
		base.OnUpdate();
	}


	public void Draw(Texture texture, Rect2 dstRect, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, dstRect, srcRect, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Rect2 Rect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, Rect, texture.Bounds, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Vect2 position, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDraw(texture, new(position, srcRect.Size), srcRect, color, origin, scale, rotation, effects, depth);
	public void Draw(Texture texture, Vect2 position, Rect2 srcRect, Color color, int depth = 0) =>
		EngineDraw(texture, new Rect2(position, srcRect.Size), srcRect, color, depth: depth);

	public void DrawText(Font font, string text, Vect2 position, Color color, int depth = 0)
		=> EngineDrawText(font, text, position, color, depth);

	public void DrawBypassAtlas(Texture texture, Rect2 dstRect, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDrawBypassAtlas(texture, dstRect, srcRect, color, origin, scale, rotation, effects, depth);
	public void DrawBypassAtlas(Texture texture, Vect2 position, Rect2 srcRect, Color color, Vect2? origin = null,
		Vect2? scale = null, float rotation = 0f, TextureEffects effects = TextureEffects.None, int depth = 0) =>
		EngineDrawBypassAtlas(texture, new Rect2(position, texture.Size), srcRect, color, origin, scale, rotation, effects, depth);
	public void DrawBypassAtlas(Texture texture, Rect2 rect, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, rect, texture.Bounds, color, depth: depth);
	public void DrawBypassAtlas(Texture texture, Vect2 position, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, new Rect2(position, texture.Size), texture.Bounds, color, depth: depth);
	public void DrawBypassAtlas(Texture texture, Rect2 dst, Rect2 src, Color color, int depth = 0) =>
		EngineDrawBypassAtlas(texture, dst, src, color, depth: depth);






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

				var quad = Renderer.DrawQuad(
					texture,
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

		var directQuad = Renderer.DrawQuad(
			texture,
			dstRect, directSrc, color,
			origin ?? Vect2.Zero, scale ?? Vect2.One,
			rotation, effects
		);

		EnqueueCommand(texture.NativeHandle, texture, directQuad, depth);
	}

	private long _seqCounter;






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

		var quad = Renderer.DrawQuad(
			texture,
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
		list.Add(new DrawCommand(texture, quad, depth, _seqCounter++));
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
		// if (!_camera.CullBounds.Intersects(dstRect))
		// 	return;

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

	protected IEnumerator WaitForRenderer(Action onReady)
	{
		yield return new WaitWhile(() => IsRendering);

		onReady?.Invoke();
	}

}
