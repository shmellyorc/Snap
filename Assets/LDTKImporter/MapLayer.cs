using System.Text.Json;

using Snap.Assets.LDTKImporter.Instances;
using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public enum MapLayerType
{
	None,
	IntGrid,
	Entities,
	Tiles,
	AutoLayer
}

public class MapLayer
{
	public string Name { get; }
	public MapLayerType Type { get; }
	public Vect2 GridSize { get; }
	public int TileSize { get; }
	public float Opacity { get; }
	public Vect2 TotalOffset { get; }
	public int TilesetId { get; }
	public string TilesetPath { get; }
	public string Id { get; }
	public int LevelId { get; }
	public Vect2 Offset { get; }
	public bool Visible { get; }
	public List<MapInstance> Instances { get; }
	public List<T> InstanceAs<T>() where T : MapInstance => Instances.OfType<T>().ToList();

	internal MapLayer(string name, MapLayerType type, Vect2 gridSize, int tileSize, float opacity,
		Vect2 totalOffset, int tilesetId, string tilesetPath, string id, int levelId, Vect2 offset,
		bool visible, List<MapInstance> instances)
	{
		Name = name;
		Type = type;
		GridSize = gridSize;
		TileSize = tileSize;
		Opacity = opacity;
		TotalOffset = totalOffset;
		TilesetId = tilesetId;
		TilesetPath = tilesetPath;
		Id = id;
		LevelId = levelId;
		Offset = offset;
		Visible = visible;
		Instances = instances;
	}

	internal static List<MapLayer> Process(JsonElement e)
	{
		var result = new List<MapLayer>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var name = t.GetPropertyOrDefault("__identifier", string.Empty);
			var type = Enum.Parse<MapLayerType>(t.GetPropertyOrDefault("__type", "None"), true);
			var cX = t.GetPropertyOrDefault<int>("__cWid");
			var cY = t.GetPropertyOrDefault<int>("__cHei");
			var tileSize = t.GetPropertyOrDefault<int>("__gridSize");
			var opacity = t.GetPropertyOrDefault<float>("__opacity");
			var totalOffsetX = t.GetPropertyOrDefault<int>("__pxTotalOffsetX");
			var totalOffsetY = t.GetPropertyOrDefault<int>("__pxTotalOffsetY");
			var tilesetId = t.GetPropertyOrDefault("__tilesetDefUid", -1);
			var tilesetPath = t.GetPropertyOrDefault("__tilesetRelPath", string.Empty);
			var id = t.GetPropertyOrDefault("iid", string.Empty);
			var levelId = t.GetPropertyOrDefault<int>("levelId");
			var offsetX = t.GetPropertyOrDefault<int>("pxOffsetX");
			var offsetY = t.GetPropertyOrDefault<int>("pxOffsetY");
			var visible = t.GetPropertyOrDefault<bool>("visible");
			var gridSize = new Vect2(cX, cY);

			List<MapInstance> instResult = type switch
			{
				MapLayerType.IntGrid => MapIntGridInstance.Process(t.GetProperty("intGridCsv"), gridSize),
				MapLayerType.Entities => MapEntityInstance.Process(t.GetProperty("entityInstances")),
				MapLayerType.Tiles => MapTileInstance.Process(t.GetProperty("gridTiles"), tileSize),
				MapLayerType.AutoLayer => MapTileInstance.Process(t.GetProperty("autoLayerTiles"), tileSize),
				_ => throw new ArgumentException($"Unable to find Map layer type, it is '{type}'.")
			};

			result.Add(
				new MapLayer(name, type, gridSize, tileSize, opacity, new(totalOffsetX, totalOffsetY),
					tilesetId, tilesetPath, id, levelId, new(offsetX, offsetY), visible, instResult)
			);
		}

		return result;
	}
}
