namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents a single int grid cell instance in a tile-based map.
/// Contains grid index data commonly used for collision or logic layers.
/// </summary>
public sealed class MapIntGridInstance : MapInstance
{
	/// <summary>
	/// The raw integer value assigned to this grid cell. Typically corresponds to a label or behavior.
	/// </summary>
	public int Index { get; }

	/// <summary>
	/// Determines whether the cell is considered solid based on its index.
	/// </summary>
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
