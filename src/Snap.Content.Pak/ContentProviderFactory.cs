using Snap.Content.Abstractions;
using Snap.Content.Abstractions.Interfaces;

namespace Snap.Content.Pak;

public static class ContentProviderFactory
{
	private sealed class EnvKeyProvider : IKeyProvider
	{
		public byte[]? GetArchiveKey()
			=> Environment.GetEnvironmentVariable("SNAP_PACK_KEY") is { Length: > 0 } hex
			   ? Convert.FromHexString(hex) : null;
	}

	// Auto-detect *.spack in contentRoot (patches first), with FS fallback.
	public static IContentProvider CreatePacksAuto(string contentRoot, IKeyProvider? key = null, Action<string>? log = null)
	{
		key ??= new EnvKeyProvider();
		var comp = new CompositeContentProvider();

		if (Directory.Exists(contentRoot))
		{
			var packs = Directory.GetFiles(contentRoot, "*.spack")
								 .OrderByDescending(Path.GetFileName); // e.g., patch_2 before patch_1

			bool anyEncrypted = false;
			bool keyMissing = key.GetArchiveKey() is null;

			foreach (var pak in packs)
			{
				var p = new PakContentProvider(pak, key);
				comp.MountFirst(p);
				anyEncrypted |= p.HasEncryptedEntries;
				log?.Invoke($"Mounted pack: {pak}");
			}

			if (anyEncrypted && keyMissing)
				log?.Invoke("WARNING: Encrypted entries detected but SNAP_PACK_KEY is not set.");
		}

		comp.MountLast(new FileSystemContentProvider(contentRoot));
		log?.Invoke($"Mounted filesystem: {contentRoot}");
		return comp;
	}

	// Explicit pack list + FS fallback
	public static IContentProvider CreatePacks(string contentRoot, IKeyProvider? key = null, Action<string>? log = null, params string[] packFiles)
	{
		key ??= new EnvKeyProvider();
		var comp = new CompositeContentProvider();

		bool anyEncrypted = false;
		bool keyMissing = key.GetArchiveKey() is null;

		foreach (var pack in packFiles)
		{
			var path = Path.Combine(contentRoot, pack);
			if (!File.Exists(path))
			{
				log?.Invoke($"Pack not found: {path}");
				continue;
			}

			var p = new PakContentProvider(path, key);
			comp.MountFirst(p);
			anyEncrypted |= p.HasEncryptedEntries;
			log?.Invoke($"Mounted pack: {path}");
		}

		if (anyEncrypted && keyMissing)
			log?.Invoke("WARNING: Encrypted entries detected but SNAP_PACK_KEY is not set.");

		comp.MountLast(new FileSystemContentProvider(contentRoot));
		log?.Invoke($"Mounted filesystem: {contentRoot}");
		return comp;
	}

	// Filesystem-only
	public static IContentProvider CreateFilesystemOnly(string contentRoot)
		=> new CompositeContentProvider(new FileSystemContentProvider(contentRoot));
}
