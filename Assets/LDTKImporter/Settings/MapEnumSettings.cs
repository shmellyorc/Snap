namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents an enumerated field value parsed from map or entity metadata.
/// Typically corresponds to a single-option dropdown or radio field in the level editor.
/// </summary>
public sealed class MapEnumSettings : MapSetting
{
	internal MapEnumSettings(string value) => Value = value;
}

/// <summary>
/// Represents an array of enumerated field values parsed from map or entity metadata.
/// Typically used for multi-select enum fields allowing multiple tags or categories.
/// </summary>
public sealed class MapEnumArraySettings : MapSetting
{
	internal MapEnumArraySettings(List<string> value) => Value = value;
}