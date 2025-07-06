using System.Text.Json;

using Snap.Assets.LDTKImporter;
using Snap.Assets.LDTKImporter.Settings;
using Snap.Systems;

namespace Snap.Helpers;

internal static class JsonHelpers
{
	public static T GetPropertyOrDefault<T>(this JsonElement parent, string propName, T defaultValue = default!)
	{
		if (parent.ValueKind != JsonValueKind.Object)
			return defaultValue;
		if (!parent.TryGetProperty(propName, out var child))
			return defaultValue;
		if (child.ValueKind == JsonValueKind.Null || child.ValueKind == JsonValueKind.Undefined)
			return defaultValue;

		var targetType = typeof(T);
		return targetType switch
		{
			Type t when t == typeof(string) => child.ValueKind == JsonValueKind.String ? (T)(object)child.GetString()! : defaultValue,
			Type t when t == typeof(int) => child.TryGetInt32(out var iValue) ? (T)(object)iValue : defaultValue,
			Type t when t == typeof(uint) => child.TryGetUInt32(out var iValue) ? (T)(object)iValue : defaultValue,
			Type t when t == typeof(float) => child.TryGetSingle(out var fValue) ? (T)(object)fValue : defaultValue,
			Type t when t == typeof(bool) =>
				(child.ValueKind == JsonValueKind.False || child.ValueKind == JsonValueKind.True) ? (T)(object)child.GetBoolean() : defaultValue,
			_ => throw new ArgumentException($"{nameof(GetPropertyOrDefault)}<{targetType.Name}> is not supported")
		};
	}

	public static Vect2 GetPosition(this JsonElement parent, string propName)
	{
		if (parent.ValueKind != JsonValueKind.Object)
			return Vect2.Zero;
		if (!parent.TryGetProperty(propName, out var child))
			return Vect2.Zero;

		var arr = child.EnumerateArray();

		return new Vect2(arr.First().GetSingle(), arr.Last().GetSingle());
	}

	public static T GetElementyOrDefault<T>(this JsonElement parent, T defaultValue = default!)
	{
		if (parent.ValueKind == JsonValueKind.Null || parent.ValueKind == JsonValueKind.Undefined)
			return defaultValue;

		var targetType = typeof(T);
		return targetType switch
		{
			Type t when t == typeof(string) => parent.ValueKind == JsonValueKind.String ? (T)(object)parent.GetString()! : defaultValue,
			Type t when t == typeof(int) => parent.TryGetInt32(out var iValue) ? (T)(object)iValue : defaultValue,
			Type t when t == typeof(uint) => parent.TryGetUInt32(out var iValue) ? (T)(object)iValue : defaultValue,
			Type t when t == typeof(float) => parent.TryGetSingle(out var fValue) ? (T)(object)fValue : defaultValue,
			Type t when t == typeof(bool) =>
				(parent.ValueKind == JsonValueKind.False || parent.ValueKind == JsonValueKind.True) ? (T)(object)parent.GetBoolean() : defaultValue,
			_ => throw new ArgumentException($"{nameof(GetPropertyOrDefault)}<{targetType.Name}> is not supported")
		};
	}

	public static Dictionary<uint, MapSetting> GetSettings(JsonElement e)
	{
		var result = new Dictionary<uint, MapSetting>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var name = t.GetPropertyOrDefault("__identifier", string.Empty);
			var type = t.GetPropertyOrDefault("__type", string.Empty);
			var value = t.GetProperty("__value");

			if (name.IsEmpty())
				throw new Exception("Map setting has a null name.");
			if (type.IsEmpty())
				throw new Exception($"Map setting has a null type from '{name}'.");

			result[HashHelpers.Hash32(name)] = type switch
			{
				// Single items:
				var x when x.StartsWith("Int") =>
					new MapIntSettings(value.GetElementyOrDefault<int>()),
				var x when x.StartsWith("Float") =>
					new MapFloatSettings(value.GetElementyOrDefault<float>()),
				var x when x.StartsWith("Bool") =>
					new MapBoolSettings(value.GetElementyOrDefault<bool>()),
				var x when x.StartsWith("String") =>
					new MapStringSettings(value.GetElementyOrDefault(string.Empty)),
				var x when x.StartsWith("Color") =>
					new MapColorSettings(new Color(value.GetElementyOrDefault("#ffffff"))),
				var x when x.StartsWith("LocalEnum.") =>
					new MapEnumSettings(value.GetElementyOrDefault(string.Empty)),
				var x when x.StartsWith("FilePath") =>
					new MapFilePathSettings(value.GetElementyOrDefault(string.Empty)),
				var x when x.StartsWith("Tile") =>
					new MapTileSettings(MapTile.Process(value)),
				var x when x.StartsWith("EntityRef") =>
					new MapEntityRefSettings(MapEntityRef.Process(value)),
				var x when x.StartsWith("Point") => new MapPointSettings(new Vect2(
						value.GetPropertyOrDefault<int>("cx"), value.GetPropertyOrDefault<int>("cy"))),

				// Array Items:
				var x when x.StartsWith("Array<Int") => new MapIntArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault<int>())
					.ToList()),
				var x when x.StartsWith("Array<Float") => new MapFloatArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault<float>())
					.ToList()),
				var x when x.StartsWith("Array<Bool") => new MapBoolArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault<bool>())
					.ToList()),
				var x when x.StartsWith("Array<String") => new MapStringArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault(string.Empty))
					.ToList()),
				var x when x.StartsWith("Array<Color") => new MapColorArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => new Color(x.GetElementyOrDefault("#ffffff")))
					.ToList()),
				var x when x.StartsWith("Array<LocalEnum.") => new MapEnumArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault(string.Empty))
					.ToList()),
				var x when x.StartsWith("Array<FilePath") => new MapEnumArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => x.GetElementyOrDefault(string.Empty))
					.ToList()),
				var x when x.StartsWith("Array<Tile") => new MapTileArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(MapTile.Process)
					.ToList()),
				var x when x.StartsWith("Array<EntityRef") => new MapEntityRefArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(MapEntityRef.Process)
					.ToList()),
				var x when x.StartsWith("Array<Point") => new MapPointArraySettings(value.EnumerateArray()
					.Where(x => x.ValueKind != JsonValueKind.Null)
					.Select(x => new Vect2(
						x.GetPropertyOrDefault<int>("cx"), x.GetPropertyOrDefault<int>("cy")))
					.ToList()),

				_ => throw new Exception($"Unable to process a map setting from '{name}' with type '{type}'.")
			};
		}

		return result;
	}
}
