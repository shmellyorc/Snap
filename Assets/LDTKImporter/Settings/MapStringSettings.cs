using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapStringSettings : MapSetting
{
	internal MapStringSettings(string value) => Value = value;
}

public sealed class MapStringArraySettings : MapSetting
{
	internal MapStringArraySettings(List<string> value) => Value = value;
}