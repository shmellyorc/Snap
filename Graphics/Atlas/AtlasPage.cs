// using SFML.Graphics;
// using SFML.System;

// public sealed class AtlasPage
// {
// 	public uint PageId { get; }
// 	public Texture GpuTexture { get; private set; }
// 	private readonly RenderTexture _rt;
// 	private readonly MaxRectsPacker _packer;
// 	private readonly List<(Texture srcTex, IntRect srcRect, IntRect dstRect)> _entries = new();
// 	private bool _needsRebuild;

// 	public int UsedPixelArea { get; private set; }
// 	public float FillRatio => (float)UsedPixelArea / (GpuTexture.Size.X * GpuTexture.Size.Y);

// 	public AtlasPage(uint pageId, int width, int height)
// 	{
// 		PageId = pageId;
// 		_packer = new MaxRectsPacker(width, height);
// 		_rt = new RenderTexture((uint)width, (uint)height);
// 		_rt.Clear(SFColor.Transparent);
// 		_rt.Display();
// 		GpuTexture = new Texture(_rt.Texture);
// 	}

// 	public IntRect? TryPack(Texture srcTexture, IntRect srcRect)
// 	{
// 		if (_needsRebuild)
// 			RebuildPage();

// 		var region = _packer.Insert(srcRect.Width, srcRect.Height);
// 		if (!region.HasValue) return null;
// 		IntRect dst = region.Value;
// 		_entries.Add((srcTexture, srcRect, dst));
// 		UsedPixelArea += dst.Width * dst.Height;

// 		// Draw the glyph region onto the RenderTexture at (dst.Left, dst.Top)
// 		var sprite = new Sprite(srcTexture)
// 		{
// 			TextureRect = srcRect,
// 			Position = new Vector2f(dst.Left, dst.Top)
// 		};
// 		_rt.Draw(sprite);
// 		_rt.Display();

// 		// Upload updated pixels into the GpuTexture
// 		GpuTexture.Update(_rt.Texture, 0, 0);
// 		return dst;
// 	}

// 	public bool RemoveLazy(IntRect dstRect)

// 	{
// 		// 1) forget that entry
// 		_entries.RemoveAll(e => e.dstRect == dstRect);
// 		UsedPixelArea -= dstRect.Width * dstRect.Height;

// 		// 2) mark free in packer
// 		_packer.RemoveLazy(dstRect);
// 		_needsRebuild = true;
// 		return true;
// 	}

// 	private void RebuildPage()
// 	{
// 		// 1) clear the RT
// 		_rt.Clear(SFColor.Transparent);
// 		_rt.Display();

// 		// 2) reset the packer (need to add Reset() to MaxRectsPacker)
// 		_packer.Reset();

// 		// 3) re‐draw every remaining entry
// 		foreach (var (tex, src, dst) in _entries)
// 		{
// 			// re‐insert into packer so free‐rectangles stay correct
// 			_packer.Insert(dst.Width, dst.Height);
// 			var sprite = new Sprite(tex)
// 			{
// 				TextureRect = src,
// 				Position = new Vector2f(dst.Left, dst.Top)
// 			};
// 			_rt.Draw(sprite);
// 		}
// 		_rt.Display();

// 		// 4) push the new atlas to GPU
// 		GpuTexture.Update(_rt.Texture, 0, 0);

// 		_needsRebuild = false;
// 	}

// 	// public IntRect? TryPack(Texture srcTexture, IntRect srcRect)
// 	// {
// 	//     var region = _packer.Insert(srcRect.Width, srcRect.Height);
// 	//     if (!region.HasValue) return null;
// 	//     IntRect dst = region.Value;
// 	//     UsedPixelArea += dst.Width * dst.Height;

// 	//     // Draw the glyph region onto the RenderTexture at (dst.Left, dst.Top)
// 	//     var sprite = new Sprite(srcTexture)
// 	//     {
// 	//         TextureRect = srcRect,
// 	//         Position = new Vector2f(dst.Left, dst.Top)
// 	//     };
// 	//     _rt.Draw(sprite);
// 	//     _rt.Display();

// 	//     // Upload updated pixels into the GpuTexture
// 	//     GpuTexture.Update(_rt.Texture, 0, 0);
// 	//     return dst;
// 	// }

// 	// public bool RemoveLazy(IntRect rect)
// 	// {
// 	//     // Deduct used area (approximate) and mark for merge next insert
// 	//     UsedPixelArea -= rect.Width * rect.Height;
// 	//     _packer.RemoveLazy(rect);
// 	//     return true;
// 	// }
// }
