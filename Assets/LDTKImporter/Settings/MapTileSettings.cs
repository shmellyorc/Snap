namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a tile reference parsed from map or entity metadata.
/// Typically used to embed single tile graphics or visual tokens within the data layer.
/// </summary>
public sealed class MapTileSettings : MapSetting
{
	internal MapTileSettings(MapTile value) => Value = value;
}

/// <summary>
/// Represents an array of tile references parsed from map or entity metadata.
/// Useful for attaching multiple tile visuals to a single field, such as randomized sets or composite graphics.
/// </summary>
public sealed class MapTileArraySettings : MapSetting
{
	internal MapTileArraySettings(List<MapTile> value) => Value = value;
}