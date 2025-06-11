using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapFilePathSettings : MapSetting
{
	internal MapFilePathSettings(string value) => Value = value;
}

public sealed class MapFilePathArraySettings : MapSetting
{
	internal MapFilePathArraySettings(List<string> value) => Value = value;
}