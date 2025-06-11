using System.Text.Json;

using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Instances;

public sealed class MapEntityInstance : MapInstance
{
	public string Name { get; }
	public Vect2 Pivot { get; }
	public string Id { get; }
	public Vect2 Size { get; }
	public Vect2 Coords { get; }
	public List<string> Tags { get; }
	public Dictionary<uint, MapSetting> Settings { get; }

	public List<TEnum> TagsAs<TEnum>() where TEnum : Enum
	{
		var result = new List<TEnum>(Tags.Count);

		for (int i = 0; i < Tags.Count; i++)
		{
			var tag = Tags[i];

			if (!Enum.TryParse(typeof(TEnum), tag, true, out var eResult))
				continue;

			result.Add((TEnum)eResult);
		}

		return result;
	}

	internal MapEntityInstance(string name, Vect2 pivot, string id, Vect2 size,
		Vect2 coords, List<string> tags, Vect2 location, Vect2 position,
		Dictionary<uint, MapSetting> settings)
		: base(location, position)
	{
		Name = name;
		Pivot = pivot;
		Id = id;
		Size = size;
		Coords = coords;
		Tags = tags;
		Settings = settings;
	}

	internal static List<MapInstance> Process(JsonElement e)
	{
		var result = new List<MapInstance>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var name = t.GetPropertyOrDefault("__identifier", string.Empty);
			var location = t.GetPosition("__grid");
			var pivot = t.GetPosition("__pivot");
			var id = t.GetPropertyOrDefault("iid", string.Empty);
			var cX = t.GetPropertyOrDefault<int>("width");
			var cY = t.GetPropertyOrDefault<int>("height");
			var position = t.GetPosition("px");
			var worldX = t.GetPropertyOrDefault<int>("__worldX");
			var worldY = t.GetPropertyOrDefault<int>("__worldY");
			var tags = t.GetProperty("__tags")
				.EnumerateArray()
				.Where(x => x.ValueKind != JsonValueKind.Null)
				.Select(x => x.GetString()!)
				.ToList();

			var settings = JsonHelpers.GetSettings(t.GetProperty("fieldInstances"));

			result.Add(new MapEntityInstance(name, pivot, id, new(cX, cY),
				new(worldX, worldY), tags, location, position, settings));
		}

		return result;
	}
}
