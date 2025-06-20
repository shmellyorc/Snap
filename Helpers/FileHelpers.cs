using System.Runtime.InteropServices;

namespace Snap.Helpers;

public static class FileHelpers
{
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

	public static void EnsureDirectoryExists(string path)
	{
		if (Directory.Exists(path))
			return;

		Directory.CreateDirectory(path);
	}

	/// <summary>
	/// Returns a writable per-user application data directory, creating it if necessary:
	///   • Windows:   %APPDATA%\[company]\[appname]  
	///   • macOS:     ~/Library/Application Support/[company]/[appname]  
	///   • Linux:     ~/.config/[company]/[appname]  
	///   • Others:    same as Linux
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
}
