using System.Text.Json;

using Snap.Enums;
using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter.Instances;

public sealed class MapTileInstance : MapInstance
{
	public Rect2 Source { get; }
	public TextureEffects Effects { get; }
	public int Tile { get; }
	public float Alpha { get; }

	internal MapTileInstance(Rect2 source, TextureEffects effects, int tile, float alpha,
		Vect2 location, Vect2 position) : base(location, position)
	{
		Source = source;
		Effects = effects;
		Tile = tile;
		Alpha = alpha;
	}

	internal static List<MapInstance> Process(JsonElement e, int tileSize)
	{
		var result = new List<MapInstance>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var position = t.GetPosition("px");
			var src = t.GetPosition("src");
			var flag = t.GetPropertyOrDefault<int>("f");
			var tile = t.GetPropertyOrDefault<int>("t");
			var alpha = t.GetPropertyOrDefault<float>("a");
			var location = Vect2.Floor(position / tileSize);
			var srcRect = new Rect2(src, new(tileSize));

			TextureEffects effects = flag switch
			{
				1 => TextureEffects.FlipHorizontal,
				2 => TextureEffects.FlipVertical,
				3 => TextureEffects.FlipHorizontal | TextureEffects.FlipVertical,
				_ => TextureEffects.None
			};

			result.Add(new MapTileInstance(srcRect, effects, tile, alpha, location, position));
		}

		return result;
	}
}
