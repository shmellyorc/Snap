using System.Text.Json;

using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Instances;

public sealed class MapIntGridInstance : MapInstance
{
	public int Index { get; }
	public bool IsSolid => Index > 0;

	internal MapIntGridInstance(int index, Vect2 location, Vect2 position) : base(location, position)
	{
		Index = index;
	}

	internal static List<MapInstance> Process(JsonElement e, Vect2 gridSize)
	{
		var result = new List<MapInstance>(e.GetArrayLength());
		var index = 0;

		foreach (var t in e.EnumerateArray())
		{
			var location = new Vect2(index % (int)gridSize.X, index / (int)gridSize.X);
			var position = gridSize * location;

			result.Add(new MapIntGridInstance(t.GetInt32(), location, position));

			index++;
		}

		return result;
	}
}
