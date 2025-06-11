using System.Text.Json;

using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public class MapLevel
{
	public string Name { get; }
	public string Id { get; }
	public Vect2 Coords { get; }
	public int WorthDepth { get; }
	public Vect2 Size { get; }
	public Vect2 GridSize { get; }
	public Color Color { get; }
	public string BgPath { get; }
	public Vect2 BgPosition { get; }
	public Vect2 BgPivot { get; }
	public List<MapLayer> Layers { get; }
	public Dictionary<uint, MapSetting> Settings { get; }

	internal MapLevel(string name, string id, Vect2 coords, int worthDepth, Vect2 size,
		Vect2 gridSize, Color color, string bgPath, Vect2 bgPosition, Vect2 bgPivot,
		List<MapLayer> layers, Dictionary<uint, MapSetting> settings)
	{
		Name = name;
		Id = id;
		Coords = coords;
		WorthDepth = worthDepth;
		Size = size;
		GridSize = gridSize;
		Color = color;
		BgPath = bgPath;
		BgPosition = bgPosition;
		BgPivot = bgPivot;
		Layers = layers;
		Settings = settings;
	}

	internal static List<MapLevel> Process(JsonElement e, int tileSize)
	{
		var result = new List<MapLevel>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			Color color;
			var name = t.GetPropertyOrDefault("identifier", string.Empty);
			var id = t.GetPropertyOrDefault("iid", string.Empty);
			var worldX = t.GetPropertyOrDefault<int>("worldX");
			var worldY = t.GetPropertyOrDefault<int>("worldY");
			var worldDepth = t.GetPropertyOrDefault<int>("worldDepth");
			var pxX = t.GetPropertyOrDefault<int>("pxWid");
			var pxY = t.GetPropertyOrDefault<int>("pxHei");
			// var color = t.GetPropertyOrDefault("bgColor", "#ffffff");
			var bgRelPath = t.GetPropertyOrDefault("bgRelPath", string.Empty);
			var bgPivotX = t.GetPropertyOrDefault<float>("bgPivotX");
			var bgPivotY = t.GetPropertyOrDefault<float>("bgPivotY");
			var size = new Vect2(pxX, pxY);
			var gridSize = Vect2.Floor(size / tileSize);
			var pxBgPos = Vect2.Zero;

			// Note: Uses __bgColor if the LDTK map color is unset; otherwise uses bgColor.
			if (t.TryGetProperty("bgColor", out var bgProp) && bgProp.ValueKind != JsonValueKind.Null)
				color = new Color(t.GetPropertyOrDefault("bgColor", "#ffffff"));
			else // Fallback to __bgColor
				color = new Color(t.GetPropertyOrDefault("__bgColor", "#ffffff"));

			if (t.TryGetProperty("bgPos", out var bgPos) && bgPos.ValueKind != JsonValueKind.Null)
			{
				var bgElem = bgPos.EnumerateArray();
				pxBgPos = new Vect2(bgElem.First().GetInt32(), bgElem.Last().GetInt32());
			}

			var settings = JsonHelpers.GetSettings(t.GetProperty("fieldInstances"));

			var layers = MapLayer.Process(t.GetProperty("layerInstances"));

			result.Add(
				new MapLevel(name, id, new(worldX, worldY), worldDepth, size, gridSize,
				color, bgRelPath, pxBgPos, new(bgPivotX, bgPivotY), layers, settings)
			);
		}

		return result;
	}
}
