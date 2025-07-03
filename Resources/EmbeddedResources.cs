using System.Reflection;

namespace Snap.Resources;

internal static class EmbeddedResources
{
    private static readonly Assembly _assembly = typeof(EmbeddedResources).Assembly;
    private const string _rootNamespace = "Snap.Resources";

    public static byte[] GetSdlDatabase()
        => ReadBytes("SdlDatabase.db");

    public static byte[] GetDefaultFont()
        => ReadBytes("DefaultFont.ttf");

    public static byte[] GetAppIcon()
        => ReadBytes("AppIcon.png");

    private static byte[] ReadBytes(string relativePath)
    {
        var resourceName = $"{_rootNamespace}.{relativePath.Replace('\\', '.').Replace('/', '.')}";
        
        using Stream? stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        return ms.ToArray();
    }
}
