namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a reference to another entity instance defined in map or level metadata.
/// Commonly used to establish links between entities, such as targets, parents, or dependencies.
/// </summary>
public sealed class MapEntityRefSettings : MapSetting
{
	internal MapEntityRefSettings(MapEntityRef value) => Value = value;
}

/// <summary>
/// Represents an array of entity references parsed from map or entity metadata.
/// Used when an entity links to multiple other instances, forming one-to-many relationships.
/// </summary>
public sealed class MapEntityRefArraySettings : MapSetting
{
	internal MapEntityRefArraySettings(List<MapEntityRef> value) => Value = value;
}