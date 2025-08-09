namespace Snap.Engine.Resources;

internal static class EmbeddedResources
{
	private static readonly Assembly Assembly = typeof(EmbeddedResources).Assembly;
	private const string RootNamespace = "Snap.Engine.Resources.Files";

	public static byte[] GetSdlDatabase() => ReadBytes("SdlDatabase.db");

	public static byte[] GetDefaultFont() => ReadBytes("DefaultFont.ttf");

	public static byte[] GetAppIcon() => ReadBytes("AppIcon.png");

	private static byte[] ReadBytes(string relativePath)
	{
		string resourceName = $"{RootNamespace}.{relativePath.Replace('\\', '.').Replace('/', '.')}";

		using Stream stream = Assembly.GetManifestResourceStream(resourceName)
			?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
		using var ms = new MemoryStream();

		stream.CopyTo(ms);

		return ms.ToArray();
	}
}
