// using System.Diagnostics;

// using SFML.Graphics;

// using Snap.Systems;
// public sealed class TextureAtlasManager
// {
// 	private readonly int _pageWidth, _pageHeight;
// 	private readonly int _maxPages;
// 	private uint _nextPageId = 0;

// 	private readonly Dictionary<uint, AtlasPage> _pagesById = new();
// 	public int Count => _registered.Count;

// 	private struct SliceInfo
// 	{
// 		public AtlasHandle Handle;
// 		// public DateTime LastUsedUtc;
// 		public DateTime LastUsedUtc;
// 	}
// 	// private readonly Dictionary<(uint nativeHandle, IntRect rect), SliceInfo> _registered
// 	// 	= new();
// 	private readonly Dictionary<(uint assetId, IntRect rect), SliceInfo> _registered
//  		= new();

// 	public Vect2 Size => new(_pageWidth, _pageHeight);
// 	public int MaxPages => _maxPages;
// 	public int PageSize => _pageWidth; // assume square pages

// 	public TextureAtlasManager(int pageSize = 2048, int maxPages = 8)
// 	{
// 		_pageWidth = pageSize;
// 		_pageHeight = pageSize;
// 		_maxPages = maxPages;
// 	}

// 	public IEnumerable<(uint assetId, SFRectI rect, DateTime lastUsedUtc)> DumpRegisteredSlices()
//     {
//         return _registered.Select(kv => (kv.Key.assetId, kv.Key.rect, kv.Value.LastUsedUtc));
//     }

// 	public AtlasHandle? GetOrCreateSlice(Snap.Assets.Loaders.Texture srcTexture, IntRect srcRect)
// 	{
// 		// var key = (srcTexture.NativeHandle, srcRect);
// 		var key = (srcTexture.Id, srcRect);
// 		if (_registered.TryGetValue(key, out var info))
// 		{
// 			// Already exists → update timestamp
// 			info.LastUsedUtc = DateTime.UtcNow;
// 			_registered[key] = info;
// 			return info.Handle;
// 		}

// 		// Try existing pages
// 		foreach (var kv in _pagesById)
// 		{
// 			var page = kv.Value;
// 			if (page.GpuTexture.Size.X == (uint)_pageWidth && page.GpuTexture.Size.Y == (uint)_pageHeight)
// 			{
// 				var region = page.TryPack(srcTexture, srcRect);
// 				if (region.HasValue)
// 				{
// 					var handle = new AtlasHandle(page.PageId, region.Value);
// 					_registered[key] = new SliceInfo
// 					{
// 						Handle = handle,
// 						LastUsedUtc = DateTime.UtcNow
// 					};
// 					return handle;
// 				}
// 			}
// 		}

// 		// Create new page if under max
// 		if (_pagesById.Count < _maxPages)
// 		{
// 			uint id = _nextPageId++;
// 			var newPage = new AtlasPage(id, _pageWidth, _pageHeight);
// 			_pagesById[id] = newPage;

// 			var newRegion = newPage.TryPack(srcTexture, srcRect).Value;
// 			var newHandle = new AtlasHandle(id, newRegion);
// 			_registered[key] = new SliceInfo
// 			{
// 				Handle = newHandle,
// 				LastUsedUtc = DateTime.UtcNow
// 			};
// 			return newHandle;
// 		}

// 		// Out of pages
// 		return null;
// 	}

// 	public Texture GetPageTexture(uint pageId)
// 	{
// 		return _pagesById[pageId].GpuTexture;
// 	}

// 	public void EvictStaleSlices(TimeSpan maxIdle)
// 	{
// 		// DateTime cutoff = DateTime.UtcNow - maxIdle;
// 		// var toEvict = new List<(uint, IntRect)>();

// 		// foreach (var kv in _registered)
// 		// {
// 		// 	if (kv.Value.LastUsedUtc < cutoff)
// 		// 		toEvict.Add(kv.Key);
// 		// }

// 		// foreach (var key in toEvict)
// 		// {
// 		// 	RemoveSlice(key.Item1, key.Item2);
// 		// 	_registered.Remove(key);
// 		// }

// 		var cutoff = DateTime.UtcNow - maxIdle;
// 		var toEvict = _registered
// 			.Where(kv => kv.Value.LastUsedUtc < cutoff)
// 			.Select(kv => kv.Key)
// 			.ToList();

// 		foreach (var key in toEvict)
// 		{
// 			_pagesById[_registered[key].Handle.PageId]
// 				.RemoveLazy(_registered[key].Handle.SourceRect);

// 			_registered.Remove(key);
// 			Console.WriteLine($"[Atlas] Evicted slice: asset={key.assetId} rect={key.rect}");
// 		}
// 	}

// 	public bool RemoveSlice(uint nativeHandle, IntRect srcRect)
// 	{
// 		// look up the *handle* we originally stored
// 		// var key = (nativeHandle, srcRect);
// 		var key = (nativeHandle, srcRect); // if you call RemoveSlice yourself, pass in texture.Id here
// 		if (!_registered.TryGetValue(key, out var info))
// 			return false;

// 		// now evict *on the exact page* and use the atlas‐coords
// 		var page = _pagesById[info.Handle.PageId];
// 		page.RemoveLazy(info.Handle.SourceRect);

// 		// finally remove from our registry
// 		_registered.Remove(key);
// 		return true;
// 	}

// 	/// <summary>
// 	/// Call this each time you draw the slice so it never ages out.
// 	/// </summary>
// 	public void TouchSlice(Snap.Assets.Loaders.Texture srcTexture, IntRect srcRect)
// 	{
// 		var key = (srcTexture.Id, srcRect);
// 		if (_registered.TryGetValue(key, out var info))
// 		{
// 			info.LastUsedUtc = DateTime.UtcNow;
// 			_registered[key] = info;
// 		}
// 	}

// 	public int PagesUsed => _pagesById.Count;
// 	public long TotalUsedPixels
// 	{
// 		get
// 		{
// 			long sum = 0;
// 			foreach (var page in _pagesById.Values)
// 				sum += page.UsedPixelArea;
// 			return sum;
// 		}
// 	}
// 	public long TotalCapacityPixels => (long)_pagesById.Count * _pageWidth * _pageHeight;

// 	public double TotalFillRatio
// 	{
// 		get
// 		{
// 			double sum = 0f;
// 			foreach (var page in _pagesById.Values)
// 				sum += page.FillRatio;

// 			if (sum == 0)
// 				return 0f;

// 			return sum / (1.0 * _pagesById.Count);
// 		}
// 	}
// }