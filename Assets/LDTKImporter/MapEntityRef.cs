using System.Text.Json;

using Snap.Helpers;

namespace Snap.Assets.LDTKImporter;

public sealed class MapEntityRef
{
	public string LayerId { get; }
	public string LevelId { get; }
	public string WorldId { get; }
	public string EntityId { get; }

	internal MapEntityRef(string entityId, string layerId, string levelId, string worldId)
	{
		EntityId = entityId;
		LayerId = layerId;
		LevelId = levelId;
		WorldId = worldId;
	}

	internal static MapEntityRef Process(JsonElement e)
	{
		var entityId = e.GetPropertyOrDefault("entityIid", string.Empty);
		var layerId = e.GetPropertyOrDefault("layerIid", string.Empty);
		var levelId = e.GetPropertyOrDefault("levelIid", string.Empty);
		var worldId = e.GetPropertyOrDefault("worldIid", string.Empty);

		return new MapEntityRef(entityId, layerId, levelId, worldId);
	}
}
