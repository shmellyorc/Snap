namespace Snap.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a file path field value parsed from map or entity metadata.
/// Typically used to reference external resources such as images, audio, or data files.
/// </summary>
public sealed class MapFilePathSettings : MapSetting
{
	internal MapFilePathSettings(string value) => Value = value;
}

/// <summary>
/// Represents an array of file path field values parsed from map or entity metadata.
/// Used when multiple external file references are provided in a single field.
/// </summary>
public sealed class MapFilePathArraySettings : MapSetting
{
	internal MapFilePathArraySettings(List<string> value) => Value = value;
}