using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a 2D point field value parsed from map or entity metadata.
/// Typically used for coordinates, positions, spawn points, or offsets.
/// </summary>
public sealed class MapPointSettings : MapSetting
{
	internal MapPointSettings(Vect2 value) => Value = value;
}

/// <summary>
/// Represents an array of 2D point field values parsed from map or entity metadata.
/// Commonly used for pathfinding nodes, patrol routes, or grouped locations.
/// </summary>
public sealed class MapPointArraySettings : MapSetting
{
	internal MapPointArraySettings(List<Vect2> value) => Value = value;
}