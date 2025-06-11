using Snap.Helpers;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public class MapSetting
{
	protected object Value { get; set; }

	public T ValueAs<T>() => (T)Value;

	public static bool GetBoolSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not bool)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(bool)}'.");

		return result.ValueAs<bool>();
	}
	public static int GetIntSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not int)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(int)}'.");

		return result.ValueAs<int>();
	}
	public static float GetFloatSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not float)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(float)}'.");

		return result.ValueAs<float>();
	}
	public static Vect2 GetPointSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not Vect2)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(Vect2)}'.");

		return result.ValueAs<Vect2>();
	}
	public static Color GetColorSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not Color)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(Color)}'.");

		return result.ValueAs<Color>();
	}
	public static string GetStringSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not string)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(string)}'.");

		return result.ValueAs<string>();
	}
	public static string GetFilePathSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not string)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(string)}'.");

		return result.ValueAs<string>();
	}
	public static MapTile GetTileSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not MapTile)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(MapTile)}'.");

		return result.ValueAs<MapTile>();
	}
	public static MapEntityRef GetEntityRefSetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not MapEntityRef)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(MapEntityRef)}'.");

		return result.ValueAs<MapEntityRef>();
	}
	public static TEnum GetEnumSetting<TEnum>(Dictionary<uint, MapSetting> settings, string name) where TEnum : Enum
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not string)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(TEnum)}'.");

		return (TEnum)Enum.Parse(typeof(TEnum), result.ValueAs<string>(), true);
	}





	public static IReadOnlyList<bool> GetBoolArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<bool>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<bool>)}'.");

		return result.ValueAs<List<bool>>();
	}
	public static IReadOnlyList<int> GetIntArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<int>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<int>)}'.");

		return result.ValueAs<List<int>>();
	}
	public static IReadOnlyList<float> GetFloatArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<float>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<float>)}'.");

		return result.ValueAs<List<float>>();
	}
	public static IReadOnlyList<Vect2> GetPointArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<Vect2>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<Vect2>)}'.");

		return result.ValueAs<List<Vect2>>();
	}
	public static IReadOnlyList<Color> GetColorArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<Color>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<Color>)}'.");

		return result.ValueAs<List<Color>>();
	}
	public static IReadOnlyList<string> GetStringArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<string>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<string>)}'.");

		return result.ValueAs<List<string>>();
	}
	public static IReadOnlyList<string> GetFilePathArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<string>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<string>)}'.");

		return result.ValueAs<List<string>>();
	}
	public static IReadOnlyList<MapTile> GetTileArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<MapTile>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<MapTile>)}'.");

		return result.ValueAs<List<MapTile>>();
	}
	public static IReadOnlyList<MapEntityRef> GetEntityRefArraySetting(Dictionary<uint, MapSetting> settings, string name)
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<MapEntityRef>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<MapEntityRef>)}'.");

		return result.ValueAs<List<MapEntityRef>>();
	}
	public static IReadOnlyList<TEnum> GetEnumArraySetting<TEnum>(Dictionary<uint, MapSetting> settings, string name) where TEnum : Enum
	{
		if (!settings.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new Exception($"Unable to find setting with the name '{name}'.");
		if (result.Value is not List<string>)
			throw new Exception($"This setting '{name}' isn't '{result.Value.GetType()}', it is  '{typeof(List<TEnum>)}'.");

		var items = result.ValueAs<List<string>>();
		var enumResult = new List<TEnum>(items.Count);

		for (int i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (!Enum.TryParse(typeof(TEnum), item, true, out var @enum))
				continue;

			enumResult.Add((TEnum)@enum);
		}

		return enumResult;
	}
}
