using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapTileSettings : MapSetting
{
	internal MapTileSettings(MapTile value) => Value = value;
}

public sealed class MapTileArraySettings : MapSetting
{
	internal MapTileArraySettings(List<MapTile> value) => Value = value;
}