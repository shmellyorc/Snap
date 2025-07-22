namespace Snap.Helpers;

/// <summary>
/// Provides helper methods for common file and path operations,
/// such as validation, directory creation, and path remapping.
/// </summary>
public static class FileHelpers
{
	/// <summary>
	/// Determines whether the specified string is a syntactically valid file path.
	/// </summary>
	/// <param name="path">The file path to validate.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="path"/> is not null or whitespace and does not contain invalid characters;
	/// otherwise, <c>false</c>.
	/// </returns>
	public static bool IsValidFilePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path)) return false;

		try
		{
			var fullPath = Path.GetFullPath(path);
			return path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Checks whether a file is currently locked (in use) by another process.
	/// </summary>
	/// <param name="path">The full path to the file to test.</param>
	/// <returns>
	/// <c>true</c> if the file exists and cannot be opened for exclusive read access;
	/// otherwise, <c>false</c>.
	/// </returns>
	public static bool IsFileInUse(string path)
	{
		if (!File.Exists(path)) return false;

		try
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
				return false;
		}
		catch
		{
			return true;
		}
	}

	/// <summary>
	/// Ensures that the directory at the specified path exists, creating it if necessary.
	/// </summary>
	/// <param name="path">The directory path to check or create.</param>
	public static void EnsureDirectoryExists(string path)
	{
		if (Directory.Exists(path))
			return;

		Directory.CreateDirectory(path);
	}

	/// <summary>
	/// Returns a writable per-user application data directory, creating it if necessary.
	/// <list type="bullet">
	///   <item><description>Windows:   %APPDATA%/[company]/[appname]</description></item>
	///   <item><description>macOS:     ~/Library/Application Support/[company]/[appname]</description></item>
	///   <item><description>Linux:     ~/.config/[company]/[appname]</description></item>
	///   <item><description>Others:    same as Linux</description></item>
	/// </list>
	/// </summary>
	/// <param name="company">Your company or organization name (optional).</param>
	/// <param name="appName">Your application’s name (required).</param>
	/// <returns>Full path to the directory.</returns>
	public static string GetApplicationData(string company, string appName)
	{
		if (string.IsNullOrWhiteSpace(appName))
			throw new ArgumentException("appName must be provided.", nameof(appName));

		string root;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			// e.g. C:\Users\Me\AppData\Roaming
			root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// e.g. /Users/me/Library/Application Support
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			root = Path.Combine(home, "Library", "Application Support");
		}
		else
		{
			// Linux or others: e.g. /home/me/.config
			var home = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
					   ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			root = Path.Combine(home, ".config");
		}

		// include optional company subfolder
		var folder = string.IsNullOrWhiteSpace(company)
			? Path.Combine(root, appName)
			: Path.Combine(root, company, appName);

		// create if missing
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);

		return folder;
	}

	/// <summary>
	/// Remaps an LDtk-relative asset path into an absolute runtime path under the application's content root.
	/// </summary>
	/// <param name="ldtkPath">The original path from the LDtk file (may contain "../" or backslashes).</param>
	/// <param name="contentRoot">The subfolder under <see cref="AppContext.BaseDirectory"/> where content is located.</param>
	/// <returns>
	/// A combined path under the application's base directory, with normalized separators and without any "../" segments.
	/// </returns>
	public static string RemapLDTKPath(string ldtkPath, string contentRoot)
    {
        string baseContentRoot = Path.Combine(AppContext.BaseDirectory, contentRoot);
        string final = ldtkPath.Replace("\\", "/").Replace("../", string.Empty);

        return Path.Combine(baseContentRoot, final);
    }
}
