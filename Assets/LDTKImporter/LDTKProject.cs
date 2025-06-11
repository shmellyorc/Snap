using System.Text.Json;

using Snap.Assets.LDTKImporter.Instances;
using Snap.Assets.Loaders;
using Snap.Helpers;
using Snap.Logs;
using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public class LDTKProject : IAsset
{
	public uint Id { get; }
	public string Tag { get; }
	public bool IsValid { get; private set; }
	public uint Handle { get; }

	// cachced levels, entities, etc:
	private Dictionary<uint, MapLevel> _levelCache = new();
	private Dictionary<uint, MapEntityInstance> _entityCache = new();
	private Dictionary<uint, MapLayer> _layerCache = new();
	private Dictionary<uint, MapTileset> _tilesetCache = new();
	private List<MapLevel> _levels;

	public IReadOnlyList<MapLevel> Levels => _levels;
	public IReadOnlyDictionary<uint, MapTileset> Tilesets => _tilesetCache;

	public LDTKProject(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}
	~LDTKProject() => Dispose();

	public ulong Load()
	{
		if (IsValid)
			return 0u;

		if (!File.Exists(Tag))
			throw new FileNotFoundException();

		var bytes = File.ReadAllBytes(Tag);
		var doc = JsonDocument.Parse(bytes);
		var root = doc.RootElement;

		var defs = root.GetProperty("defs");
		var defTilesets = defs.GetProperty("tilesets");

		_tilesetCache = MapTileset.Process(defTilesets);
		_levels = MapLevel.Process(root.GetProperty("levels"),
			root.GetPropertyOrDefault<int>("defaultGridSize"));

		_levelCache = new Dictionary<uint, MapLevel>(_levels.Count);
		for (int i = 0; i < _levels.Count; i++)
		{
			var level = _levels[i];

			_levelCache[HashHelpers.Hash32(level.Id)] = level;

			for (int x = 0; x < level.Layers.Count; x++)
			{
				var layer = level.Layers[x];

				_layerCache[HashHelpers.Hash32(layer.Id)] = layer;

				if (layer.Type == MapLayerType.Entities)
				{
					var instances = layer.InstanceAs<MapEntityInstance>();
					for (int z = 0; z < instances.Count; z++)
					{
						var entity = instances[z];

						_entityCache[HashHelpers.Hash32(entity.Id)] = entity;
					}
				}
			}
		}

		IsValid = true;

		return (ulong)bytes.Length;
	}

	public void Unload()
	{
		if (!IsValid)
			return;

		Dispose();

		IsValid = false;
	}

	public void Dispose()
	{
		_levels?.Clear();
		_levelCache?.Clear();
		_entityCache?.Clear();
		_layerCache?.Clear();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");
	}



	#region CreateInstance
	public static bool TryCreateInstance<T>(out T instance, string name, bool ignoreCase = true, params object[] args)
	{
		instance = CreateInstance<T>(name, ignoreCase, args);

		return instance != null;
	}

	public static T CreateInstance<T>(string name, bool ignoreCase, params object[] args)
	{
		if (name.IsEmpty())
			return default!;

		var ap = AppDomain.CurrentDomain.GetAssemblies();
		var ignoreType = ignoreCase
			 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

		for (int i = ap.Length - 1; i >= 0; i--)
		{
			var asm = ap[i];
			var foundType = asm.GetType(name, false, ignoreCase);

			if (foundType == null)
			{
				foundType = asm.GetTypes()
					.FirstOrDefault(x => string.Equals(x.Name, name, ignoreType));
			}

			if (foundType == null)
				continue;

			if (!typeof(T).IsAssignableFrom(foundType))
				continue;

			return (T)Activator.CreateInstance(foundType, args)!;
		}

		return default;
	}
	#endregion


	#region Helpers
	public static Vect2 MapToWorld(in Vect2 location, int tilesize)
		=> Vect2.Floor(location * tilesize);
	public static Vect2 WorldToMap(in Vect2 position, int tilesize)
		=> Vect2.Floor(position / tilesize);


	#endregion


	#region Entity
	public MapEntityInstance GetEntityByHash(uint hash)
	{
		if (_entityCache.Count == 0)
			throw new Exception("You don't have any entities to search");

		if (!_entityCache.TryGetValue(hash, out var entity))
			throw new Exception($"Unable to find a map entity with the hash id '{hash}'.");

		return entity;
	}
	public MapEntityInstance GetEntityById(string id)
	{
		if (_entityCache.Count == 0)
			throw new Exception("You don't have any entities to search");

		if (!_entityCache.TryGetValue(HashHelpers.Hash32(id), out var entity))
			throw new Exception($"Unable to find a entity with the id '{id}'.");

		return entity;
	}
	#endregion


	#region Layer
	public MapLayer GetLayerByHash(uint hash)
	{
		if (_layerCache.Count == 0)
			throw new Exception("You don't have any layers to search");

		if (!_layerCache.TryGetValue(hash, out var layer))
			throw new Exception($"Unable to find a map layer with the hash id '{hash}'.");

		return layer;
	}
	public MapLayer GetLayerById(string id)
	{
		if (_layerCache.Count == 0)
			throw new Exception("You don't have any layers to search");

		if (!_layerCache.TryGetValue(HashHelpers.Hash32(id), out var layer))
			throw new Exception($"Unable to find a layer with the id '{id}'.");

		return layer;
	}
	#endregion


	#region Levels
	public bool TryGetLevelByHash(uint hash, out MapLevel level)
	{
		level = GetLevelByHash(hash);

		return level != null;
	}
	public MapLevel GetLevelByHash(uint hash)
	{
		if (_layerCache.Count == 0)
			throw new Exception("You don't have any map levels to search");

		if (!_levelCache.TryGetValue(hash, out var level))
			return null;

		return level;
	}

	public bool TryGetLevelById(string id, out MapLevel level)
	{
		level = GetLevelById(id);

		return level != null;
	}
	public MapLevel GetLevelById(string id)
	{
		if (_layerCache.Count == 0)
			throw new Exception("You don't have any map levels to search");

		if (!_levelCache.TryGetValue(HashHelpers.Hash32(id), out var level))
			return null;

		return level;
	}

	public bool TryGetLevelByName(string name, bool ignoreCase, out MapLevel level)
	{
		level = GetLevelByName(name, ignoreCase);

		return level != null;
	}
	public MapLevel GetLevelByName(string name, bool ignoreCase)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Is null or empty");
		if (_tilesetCache.Count == 0)
			throw new Exception("You don't have any map levels to search");

		var type = ignoreCase
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		foreach (var lv in _levels)
		{
			if (lv.Name.Equals(name, type))
				return lv;
		}

		return null;
	}
	#endregion


	#region Tileset
	public MapTileset GetTilesetByHash(uint hash)
	{
		if (_tilesetCache.Count == 0)
			throw new Exception("You don't have any tilesets to search");

		if (!_tilesetCache.TryGetValue(hash, out var tilemap))
			throw new Exception($"Unable to find a tileset with the hash id '{hash}'.");

		return tilemap;
	}
	public MapTileset GetTilesetByName(string name, bool ignoreCase)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Is null or empty");
		if (_tilesetCache.Count == 0)
			throw new Exception("You don't have any tilesets to search");

		var type = ignoreCase
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		foreach (var kv in _tilesetCache)
		{
			if (kv.Value.Name.Equals(name, type))
				return kv.Value;
		}

		throw new Exception($"Unable to find a tileset called '{name}'.");
	}
	#endregion
}
