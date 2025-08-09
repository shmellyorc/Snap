namespace Snap.Engine.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a boolean field value parsed from map or entity metadata.
/// Typically corresponds to a user-defined checkbox or toggle in the level editor.
/// </summary>
public sealed class MapBoolSettings : MapSetting
{
	internal MapBoolSettings(bool value) => Value = value;
}

/// <summary>
/// Represents an array of boolean field values parsed from map or entity metadata.
/// Typically corresponds to a multi-checkbox field or list of flags in the level editor.
/// </summary>
public sealed class MapBoolArraySettings : MapSetting
{
	internal MapBoolArraySettings(List<bool> value) => Value = value;
}