using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapEnumSettings : MapSetting
{
	internal MapEnumSettings(string value) => Value = value;
}

public sealed class MapEnumArraySettings : MapSetting
{
	internal MapEnumArraySettings(List<string> value) => Value = value;
}