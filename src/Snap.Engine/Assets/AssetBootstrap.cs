using Snap.Content.Abstractions;

namespace Snap.Engine.Assets;

public static class AssetBootstrap
{
	/// <summary>
	/// Initialize the content system with a default FileSystem provider so things "just work".
	/// </summary>
	/// <param name="contentRoot">
	/// Content folder relative to the app base. If null, uses EngineSettings.Instance.AppContentRoot.
	/// </param>
	/// <param name="warnIfEmpty">Log a warning if no files are found.</param>
	public static void InitDefault(string? contentRoot = null, bool warnIfEmpty = true)
	{
		contentRoot ??= EngineSettings.Instance.AppContentRoot;

		var composite = new CompositeContentProvider(
			new FileSystemContentProvider(contentRoot)
		);

		AssetManager.SetProvider(composite, disposeOld: true, clearCaches: true);

		if (warnIfEmpty)
		{
			// Soft check: don’t crash first-time users—just nudge them.
			if (!composite.List("").Any())
			{
				Logger.Instance?.Log(LogLevel.Warning,
					$"No content found under '{contentRoot}'. " +
					$"Add assets there (e.g., Graphics/, Sound/, etc.).");
			}
			else
			{
				Logger.Instance?.Log(LogLevel.Info,
					$"Content provider initialized (filesystem): '{contentRoot}'.");
			}
		}
	}

	/// <summary>
	/// Swap to a different content root at runtime (still filesystem-based).
	/// </summary>
	public static void SwitchContentRoot(string newContentRoot)
	{
		var composite = new CompositeContentProvider(
			new FileSystemContentProvider(newContentRoot)
		);
		AssetManager.SetProvider(composite, disposeOld: true, clearCaches: true);
		Logger.Instance?.Log(LogLevel.Info, $"Switched content root to '{newContentRoot}'.");
	}
}