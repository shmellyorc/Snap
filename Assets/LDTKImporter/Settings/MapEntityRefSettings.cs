using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Settings;

public sealed class MapEntityRefSettings : MapSetting
{
	internal MapEntityRefSettings(MapEntityRef value) => Value = value;
}

public sealed class MapEntityRefArraySettings : MapSetting
{
	internal MapEntityRefArraySettings(List<MapEntityRef> value) => Value = value;
}