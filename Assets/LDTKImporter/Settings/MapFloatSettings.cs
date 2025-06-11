namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapFloatSettings : MapSetting
{
	internal MapFloatSettings(float value) => Value = value;
}

public sealed class MapFloatArraySettings : MapSetting
{
	internal MapFloatArraySettings(List<float> value) => Value = value;
}