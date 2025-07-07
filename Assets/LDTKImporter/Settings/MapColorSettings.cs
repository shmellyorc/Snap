namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a color field value parsed from entity or level metadata.
/// Typically corresponds to a color picker field in the level editor.
/// </summary>
public sealed class MapColorSettings : MapSetting
{
	internal MapColorSettings(Color value) => Value = value;
}

/// <summary>
/// Represents an array of color field values parsed from entity or level metadata.
/// Typically corresponds to a multi-color selection field in the level editor.
/// </summary>
public sealed class MapColorArraySettings : MapSetting
{
	internal MapColorArraySettings(List<Color> value) => Value = value;
}