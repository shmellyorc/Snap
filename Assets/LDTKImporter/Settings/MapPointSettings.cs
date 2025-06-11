using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapPointSettings : MapSetting
{
	internal MapPointSettings(Vect2 value) => Value = value;
}

public sealed class MapPointArraySettings : MapSetting
{
	internal MapPointArraySettings(List<Vect2> value) => Value = value;
}