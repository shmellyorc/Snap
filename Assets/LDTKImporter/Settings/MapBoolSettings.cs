namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapBoolSettings : MapSetting
{
	internal MapBoolSettings(bool value) => Value = value;
}

public sealed class MapBoolArraySettings : MapSetting
{
	internal MapBoolArraySettings(List<bool> value) => Value = value;
}