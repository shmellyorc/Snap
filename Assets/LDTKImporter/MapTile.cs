using System.Text.Json;

using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public sealed class MapTile
{
	public int TilesetId { get; }
	public Rect2 Source { get; }

	internal MapTile(int tilesetId, Rect2 source)
	{
		TilesetId = tilesetId;
		Source = source;
	}

	internal static MapTile Process(JsonElement e)
	{
		var tilesetId = e.GetPropertyOrDefault<int>("tilesetUid");
		var x = e.GetPropertyOrDefault<int>("x");
		var y = e.GetPropertyOrDefault<int>("y");
		var w = e.GetPropertyOrDefault<int>("w");
		var h = e.GetPropertyOrDefault<int>("h");

		return new MapTile(tilesetId, new(x, y, w, h));
	}
}
