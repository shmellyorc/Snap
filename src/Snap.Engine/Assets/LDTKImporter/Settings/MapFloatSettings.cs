namespace Snap.Engine.Assets.LDTKImporter.Settings;

/// <summary>
/// Represents a floating-point field value parsed from map or entity metadata.
/// Typically used for configurable numeric properties such as speed, duration, or scale.
/// </summary>
public sealed class MapFloatSettings : MapSetting
{
	internal MapFloatSettings(float value) => Value = value;
}

/// <summary>
/// Represents an array of floating-point field values parsed from map or entity metadata.
/// Useful for multi-value numeric inputs or lists of adjustable values.
/// </summary>
public sealed class MapFloatArraySettings : MapSetting
{
	internal MapFloatArraySettings(List<float> value) => Value = value;
}