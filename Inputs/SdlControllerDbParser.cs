using System.Text;
using Snap.Resources;

namespace Snap.Inputs;

internal static class SdlControllerDbParser
{
    public static List<SdlControllerEntry> LoadAll()
    {
        byte[] raw = EmbeddedResources.GetSdlDatabase();
        string text = Encoding.UTF8.GetString(raw);
        var list = new List<SdlControllerEntry>();

        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#') continue;

            var parts = line.Split([','], 3);
            if (parts.Length < 3) continue;

            var map = ParseMap(parts[2]);
            list.Add(new SdlControllerEntry(parts[0], parts[1], map));
        }
        return list;
    }

    private static Dictionary<char, int> ParseMap(string mapping)
    {
        var dict = new Dictionary<char, int>();

        foreach (var token in mapping.Split(','))
        {
            var kv = token.Split(':', 2);
            if (kv.Length != 2) continue;
            if (int.TryParse(kv[1], out var idx))
                dict[kv[0][0]] = idx;
        }
        
        return dict;
    }
}
