namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a string field value parsed from map or entity metadata.
/// Commonly used for names, identifiers, instructions, or dialogue content.
/// </summary>
public sealed class MapStringSettings : MapSetting
{
	internal MapStringSettings(string value) => Value = value;
}

/// <summary>
/// Represents an array of string field values parsed from map or entity metadata.
/// Useful for multi-line text, tag groups, or custom string lists.
/// </summary>
public sealed class MapStringArraySettings : MapSetting
{
	internal MapStringArraySettings(List<string> value) => Value = value;
}