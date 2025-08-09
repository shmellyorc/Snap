using System.Collections.Concurrent;

using Snap.Content.Abstractions.Interfaces;

namespace Snap.Content.Abstractions;

public class CompositeContentProvider : IContentProvider, IDisposable
{
	private readonly List<IContentProvider> _providers = new();
	private readonly ConcurrentDictionary<string, IContentProvider> _hitCache = new(StringComparer.OrdinalIgnoreCase);
	private readonly object _gate = new();

	public CompositeContentProvider(params IContentProvider[] providers)
	{
		if (providers is not null)
			_providers.AddRange(providers);
	}

	private static string Norm(string p) => p.Replace('\\', '/').TrimStart('/');

	// Mount at the *front* (highest priority; patches should go here)
	public CompositeContentProvider MountFirst(IContentProvider p)
	{
		lock (_gate) { _providers.Insert(0, p); _hitCache.Clear(); }
		return this;
	}

	// Mount at the *end* (lowest priority; base packs here)
	public CompositeContentProvider MountLast(IContentProvider p)
	{
		lock (_gate) { _providers.Add(p); _hitCache.Clear(); }
		return this;
	}

	public bool Unmount(IContentProvider p)
	{
		lock (_gate)
		{
			var removed = _providers.Remove(p);
			if (removed) _hitCache.Clear();
			return removed;
		}
	}

	public void ClearCache() => _hitCache.Clear();


	public bool Exists(string path)
	{
		var norm = Norm(path);
		if (_hitCache.ContainsKey(norm)) return true;

		foreach (var p in Snapshot())
		{
			if (p.Exists(norm)) { _hitCache[norm] = p; return true; }
		}
		return false;
	}

	public Stream OpenRead(string path)
	{
		var norm = Norm(path);
		if (_hitCache.TryGetValue(norm, out var cached))
			return cached.OpenRead(norm);

		foreach (var p in Snapshot())
		{
			if (p.Exists(norm))
			{
				_hitCache[norm] = p;
				return p.OpenRead(norm);
			}
		}
		throw new FileNotFoundException(norm);
	}

	// Merge listings with first-hit-wins semantics
	public IEnumerable<string> List(string folder)
	{
		var norm = Norm(folder);
		if (norm.Length > 0 && !norm.EndsWith('/')) norm += '/';

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var p in Snapshot())
		{
			foreach (var item in p.List(norm))
			{
				var n = Norm(item);
				if (!n.StartsWith(norm, StringComparison.OrdinalIgnoreCase)) continue;

				if (seen.Add(n))
					yield return n; // first provider to expose this path "owns" it
			}
		}
	}

	private IContentProvider[] Snapshot()
	{
		lock (_gate) return _providers.ToArray();
	}

	public void Dispose()
	{
		lock (_gate)
		{
			foreach (var p in _providers.OfType<IDisposable>())
				p.Dispose();
			_providers.Clear();
			_hitCache.Clear();
		}
	}
}
