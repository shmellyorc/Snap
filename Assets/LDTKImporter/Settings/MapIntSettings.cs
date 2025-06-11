namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapIntSettings : MapSetting
{
	internal MapIntSettings(int value) => Value = value;
}

public sealed class MapIntArraySettings : MapSetting
{
	internal MapIntArraySettings(List<int> value) => Value = value;
}