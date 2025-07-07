namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents an integer field value parsed from map or entity metadata.
/// Commonly used for enumerations, counters, or discrete value options.
/// </summary>
public sealed class MapIntSettings : MapSetting
{
	internal MapIntSettings(int value) => Value = value;
}

/// <summary>
/// Represents an array of integer field values parsed from map or entity metadata.
/// Useful for defining lists of levels, category IDs, or other multi-int values.
/// </summary>
public sealed class MapIntArraySettings : MapSetting
{
	internal MapIntArraySettings(List<int> value) => Value = value;
}