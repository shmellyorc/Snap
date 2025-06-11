using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapColorSettings : MapSetting
{
	internal MapColorSettings(Color value) => Value = value;
}

public sealed class MapColorArraySettings : MapSetting
{
	internal MapColorArraySettings(List<Color> value) => Value = value;
}