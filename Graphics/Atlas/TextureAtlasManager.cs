using Snap.Logs;

public sealed class TextureAtlasManager
{
	private readonly Dictionary<int, AtlasPage> _pagesById = new();
	private readonly Dictionary<(uint texHandle, SFRectI rect), SliceInfo> _registered = new();
	public TimeSpan MaxIdle { get; set; }

	public static TextureAtlasManager Instance { get; private set; }
	public int PageSize { get; }
	public int MaxPages { get; }
	public int Pages => _pagesById.Count;

	public double TotalFillRatio
		=> TotalCapacityPixels == 0
		   ? 0
		   : (double)TotalUsedPixels / TotalCapacityPixels;

	private long TotalCapacityPixels
		=> (long)PageSize * PageSize * _pagesById.Count;

	private long TotalUsedPixels
		=> _pagesById.Values.Sum(p => p.UsedPixels);

	internal TextureAtlasManager(int pageSize, int maxPages)
	{
		Instance ??= this;

		PageSize = pageSize;
		MaxPages = maxPages;
		MaxIdle = TimeSpan.FromMinutes(30);

		// start with one page (id = 0)
		_pagesById[0] = new AtlasPage(pageSize, 0);
	}

	// For debugging / metrics
	public int Count => _registered.Count;

	// Internal record per slice
	private struct SliceInfo
	{
		public AtlasHandle Handle;
		public DateTime LastUsedUtc;
	}

	/// <summary>
	/// Main entry: get an existing slice or pack it in. On failure,
	/// evict stale, retry; on further failure do LRU eviction until it fits.
	/// </summary>
	public AtlasHandle? GetOrCreateSlice(SFTexture srcTexture, SFRectI srcRect)
	{
		var key = (srcTexture.NativeHandle, srcRect);

		// 0) EVICT STALE ON *EVERY* ACCESS (must be before cache lookup)
		var evicted = EvictStaleSlices(MaxIdle);
		if (evicted.Count > 0)
			Logger.Instance.Log(LogLevel.Info, $"[Atlas] ⚠️ Evicted {evicted.Count} stale slices at {DateTime.UtcNow:HH:mm:ss}");

		// 1) Cache hit?
		if (_registered.TryGetValue(key, out var info))
		{
			info.LastUsedUtc = DateTime.UtcNow;
			_registered[key] = info;
			return info.Handle;
		}

		AtlasHandle handle;

		// 2) Try packing straight away (no eviction)
		if (TryPackIntoAnyPage(srcTexture, srcRect, out handle))
		{
			_registered[key] = new SliceInfo { Handle = handle, LastUsedUtc = DateTime.UtcNow };
			return handle;
		}

		// 3) Evict truly idle slices
		// EvictStaleSlices(MaxIdle);

		if (TryPackIntoAnyPage(srcTexture, srcRect, out handle))
		{
			_registered[key] = new SliceInfo { Handle = handle, LastUsedUtc = DateTime.UtcNow };
			return handle;
		}

		// 4) LRU eviction: remove oldest one by one until it fits
		var lruList = _registered
			.OrderBy(kv => kv.Value.LastUsedUtc)
			.Select(kv => kv.Key)
			.ToList();

		foreach (var evictKey in lruList)
		{
			// remove from page & registry
			RemoveSlice(evictKey.texHandle, evictKey.rect);

			if (TryPackIntoAnyPage(srcTexture, srcRect, out handle))
			{
				_registered[key] = new SliceInfo { Handle = handle, LastUsedUtc = DateTime.UtcNow };
				return handle;
			}
		}

		// 5) Still no room
		return null;
	}

	/// <summary>
	/// Attempts a pack on every existing page, or creates a new one if under maxPages.
	/// </summary>
	private bool TryPackIntoAnyPage(SFTexture srcTexture, SFRectI srcRect, out AtlasHandle handle)
	{
		// 1) Existing pages
		foreach (var page in _pagesById.Values)
		{
			if (page.TryPack(srcTexture, srcRect, out handle))
				return true;
		}

		// 2) New page
		if (_pagesById.Count < MaxPages)
		{
			int newId = _pagesById.Count;
			var page = new AtlasPage(PageSize, newId);
			_pagesById[newId] = page;

			if (page.TryPack(srcTexture, srcRect, out handle))
				return true;
		}

		handle = default;
		return false;
	}

	/// <summary>
	/// Evicts any slice not used in the last `maxIdle` span.
	/// Returns the list of evicted keys for logging or metrics.
	/// </summary>
	public List<(uint texHandle, SFRectI rect)> EvictStaleSlices(TimeSpan maxIdle)
	{
		var cutoff = DateTime.UtcNow - maxIdle;
		var toEvict = _registered
			.Where(kv => kv.Value.LastUsedUtc < cutoff)
			.Select(kv => kv.Key)
			.ToList();

		foreach (var key in toEvict)
		{
			var info = _registered[key];
			_pagesById[info.Handle.PageId]
				.RemoveLazy(info.Handle.SourceRect);
			_registered.Remove(key);
		}

		return toEvict;
	}

	/// <summary>
	/// Immediately evict one slice (called during LRU loop).
	/// </summary>
	public bool RemoveSlice(uint texHandle, SFRectI rect)
	{
		var key = (texHandle, rect);
		if (!_registered.TryGetValue(key, out var info))
			return false;

		_pagesById[info.Handle.PageId]
			.RemoveLazy(info.Handle.SourceRect);
		_registered.Remove(key);
		return true;
	}

	/// <summary>
	/// Fetch the SFTexture for a given page.
	/// </summary>
	public SFTexture GetPageTexture(int pageId)
	{
		if (!_pagesById.TryGetValue(pageId, out var page))
			throw new ArgumentOutOfRangeException(nameof(pageId));
		return page.Texture;
	}



	public void UnloadAsset(uint texHandle)
	{
		// find every key for that handle
		var keys = _registered.Keys
			.Where(k => k.texHandle == texHandle)
			.ToList();

		foreach (var key in keys)
		{
			// remove from registry and page
			var info = _registered[key];
			_pagesById[info.Handle.PageId]
				.RemoveLazy(info.Handle.SourceRect);
			_registered.Remove(key);
		}
	}
}