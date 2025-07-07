namespace Snap.Assets.Loaders;

public sealed class AssetManager
{
    private sealed class AssetEntry
    {
        public IAsset Asset { get; }
        public DateTime LastAccessFrame { get; set; }
        public ulong Length { get; set; }

        public AssetEntry(IAsset asset)
        {
            Asset = asset;
            LastAccessFrame = DateTime.UtcNow;
        }
    }

    private const long EvictAfterMinutes = 15;

    private readonly Dictionary<uint, AssetEntry> _assets = new(32);

    private static readonly string[] TextureExtentions = {
        ".png", ".bmp", ".tga", ".jpg", ".gif", ".psd", ".hdr", ".pic", ".pnm" };
    private static readonly string[] SpriteFontExtentions = {
        ".png", ".bmp", ".tga", ".jpg", ".gif", ".psd", ".hdr", ".pic", ".pnm" };
    private static readonly string[] LDTKExtentions = { ".ldtk", ".json" };
    private static readonly string[] SpritesheetExtentions = { ".sheet", ".json" };
    private static readonly string[] SoundExtentions = {
        ".wav", ".mp3", ".ogg", ".flac", ".aiff", ".au", ".raw", ".paf", ".svx",
        ".nist", ".voc", ".ircam", ".w64", ".mat4", ".mat5", ".pvf", ".htk",
        ".sds", ".avr", ".sd2", ".caf", ".wve", ".mpc2k", ".rf64"
    };

    internal static uint Id { get; set; } = 1u;

    public static AssetManager Instance { get; private set; }
    public int TotalCount => _assets.Count;
    public long BytesLoaded { get; private set; }
    public int Count => _assets.Count(x => x.Value.Asset.IsValid);

    internal AssetManager() => Instance ??= this;

    public void Add(Enum name, IAsset asset) => Add(name.ToEnumString(), asset);
    public void Add(string name, IAsset asset)
    {
        var hash = HashHelpers.Hash32(name);
        if (_assets.ContainsKey(hash))
            throw new InvalidOperationException($"An asset with the name '{name}' already exists.");

        _assets[hash] = new AssetEntry(asset);
    }


    public void Remove(Enum name) => Remove(name.ToEnumString());
    public void Remove(string name) =>
        InternalRemove(HashHelpers.Hash32(name), true);


    public T Get<T>(Enum name) where T : IAsset => Get<T>(name.ToEnumString());
    public T Get<T>(string name) where T : IAsset
    {
        var hash = HashHelpers.Hash32(name);
        if (!_assets.TryGetValue(hash, out var entry))
            throw new KeyNotFoundException($"No asset found for '{name}'.");

        entry.LastAccessFrame = DateTime.UtcNow;

        EvictAssets();

        if (!entry.Asset.IsValid)
        {
            entry.Length = entry.Asset.Load();
            BytesLoaded += (long)entry.Length;

            Logger.Instance.Log(LogLevel.Info, $"Loaded asset with ID: {entry.Asset.Id}, type: '{entry.Asset.GetType().Name}'.");
        }

        return (T)entry.Asset;
    }

    public bool TryGet<T>(Enum name, out T asset) where T : IAsset =>
        TryGet(name.ToEnumString(), out asset);
    public bool TryGet<T>(string name, out T asset) where T : IAsset
    {
        asset = Get<T>(name);

        return asset != null;
    }

    public void UnloadAll()
    {
        foreach (var item in _assets)
            InternalRemove(item.Key, false);
    }

    internal void Clear()
    {
        foreach (var item in _assets)
            InternalRemove(item.Key, true);
        _assets.Clear();
    }

    #region Loaders
    public static Texture LoadTexture(string filename, bool repeat = false, bool smooth = false)
    {
        if (!TryFindFullPath(filename, TextureExtentions, out var fullPath))
            throw new FileNotFoundException($"Texture file '{filename}' could not be found.");

        return new Texture(Id++, fullPath, repeat, smooth);
    }

    public static SpriteFont LoadSpriteFont(string filename, float spacing = 0f, float lineSpacing = 0f)
    {
        if (!TryFindFullPath(filename, SpriteFontExtentions, out var fullPath))
            throw new FileNotFoundException($"Sprite font file '{filename}' could not be found.");

        return new SpriteFont(Id++, fullPath, spacing, lineSpacing);
    }

    public static LDTKProject LoadMap(string filename)
    {
        if (!TryFindFullPath(filename, LDTKExtentions, out var fullPath))
            throw new FileNotFoundException($"Map font file '{filename}' could not be found.");

        return new LDTKProject(Id++, fullPath);
    }

    public static Spritesheet LoadSheet(string filename)
    {
        if (!TryFindFullPath(filename, SpritesheetExtentions, out var fullPath))
            throw new FileNotFoundException($"Spritesheet font file '{filename}' could not be found.");

        return new Spritesheet(Id++, fullPath);
    }

    public static Sound LoadSound(string filename, bool looped = false)
    {
        if (!TryFindFullPath(filename, SoundExtentions, out var fullPath))
            throw new FileNotFoundException($"Sound font file '{filename}' could not be found.");

        return new Sound(Id++, fullPath, looped);
    }
    #endregion


    #region Getters:
    public static Texture GetTexture(string name) => Instance.Get<Texture>(name);
    public static Texture GetTexture(Enum name) => Instance.Get<Texture>(name);
    public static bool TryGetTexture(string name, out Texture asset) => Instance.TryGet(name, out asset);
    public static bool TryGetTexture(Enum name, out Texture asset) => Instance.TryGet(name, out asset);

    public static LDTKProject GetMap(string name) => Instance.Get<LDTKProject>(name);
    public static LDTKProject GetMap(Enum name) => Instance.Get<LDTKProject>(name);
    public static bool TryGetMap(string name, out LDTKProject asset) => Instance.TryGet(name, out asset);
    public static bool TryGetMap(Enum name, out LDTKProject asset) => Instance.TryGet(name, out asset);

    public static Spritesheet GetSheet(string name) => Instance.Get<Spritesheet>(name);
    public static Spritesheet GetSheet(Enum name) => Instance.Get<Spritesheet>(name);
    public static bool TryGetSheet(string name, out Spritesheet asset) => Instance.TryGet(name, out asset);
    public static bool TryGetSheet(Enum name, out Spritesheet asset) => Instance.TryGet(name, out asset);

    public static Font GetFont(string name) => Instance.Get<Font>(name);
    public static Font GetFont(Enum name) => Instance.Get<Font>(name);
    public static bool TryGetFont(string name, out Font asset) => Instance.TryGet(name, out asset);
    public static bool TryGetFont(Enum name, out Font asset) => Instance.TryGet(name, out asset);

    public static BitmapFont GetBitmapFont(string name) => Instance.Get<BitmapFont>(name);
    public static BitmapFont GetBitmapFont(Enum name) => Instance.Get<BitmapFont>(name);
    public static bool TryGetBitmapFont(string name, out BitmapFont asset) => Instance.TryGet(name, out asset);
    public static bool TryGetBitmapFont(Enum name, out BitmapFont asset) => Instance.TryGet(name, out asset);

    public static SpriteFont GetSpriteFont(string name) => Instance.Get<SpriteFont>(name);
    public static SpriteFont GetSpriteFont(Enum name) => Instance.Get<SpriteFont>(name);
    public static bool TryGetSpriteFont(string name, out SpriteFont asset) => Instance.TryGet(name, out asset);
    public static bool TryGetSpriteFont(Enum name, out SpriteFont asset) => Instance.TryGet(name, out asset);

    public static Sound GetSound(string name) => Instance.Get<Sound>(name);
    public static Sound GetSound(Enum name) => Instance.Get<Sound>(name);
    public static bool TryGetSound(string name, out Sound asset) => Instance.TryGet(name, out asset);
    public static bool TryGetSound(Enum name, out Sound asset) => Instance.TryGet(name, out asset);
    #endregion


    #region TilesetTexture


    public Texture GetTilesetTexture(LDTKProject project, int tilesetId)
    {
        // -1 = happens when the LDTK layer has no tileset on that insance layer. 
        // 
        // Usually happens when no tilset is set OR the layer isnt a tile instance.

        if (tilesetId == -1)
            throw new InvalidOperationException("Tileset ID is -1. This usually means the layer has no tilset assigned or is not a tile instance layer.");
        if (!project.Tilesets.TryGetValue(tilesetId, out var tileset))
            throw new KeyNotFoundException($"Tileset with ID {tilesetId} was not found in LDTK project");

        string ldtlPath = FileHelpers.RemapLDTKPath(tileset.Path, EngineSettings.Instance.AppContentRoot);

        Texture texture = _assets
            .Where(x => x.Value.Asset is Texture)
            .Select(x => x.Value.Asset as Texture)
            .FirstOrDefault(x => Path.GetFullPath(x.Tag).Equals(ldtlPath, StringComparison.OrdinalIgnoreCase));

        return texture;
    }
    public bool TryGetTilesetTexture(LDTKProject project, int TilesetID, out Texture texture)
    {
        texture = GetTilesetTexture(project, TilesetID);

        return texture != null;
    }
    #endregion


    #region Private Methods
    private static bool TryFindFullPath(string assetPathWithoutExtention, string[] extentions,
    out string foundFullPath)
    {
        if (string.IsNullOrEmpty(assetPathWithoutExtention))
        {
            foundFullPath = null;
            return false;
        }

        if (extentions == null || extentions.Length == 0)
        {
            foundFullPath = null;
            return false;
        }

        string baseDir = AppContext.BaseDirectory;
        string contentFolder = Path.Combine(baseDir, EngineSettings.Instance.AppContentRoot);
        string normalizeAssetPath = assetPathWithoutExtention
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        foreach (var ext in extentions)
        {
            if (string.IsNullOrEmpty(ext))
                continue;
            string normalizeExt = ext.StartsWith('.') ? ext : $".{ext}";

            string candidate = Path.Combine(
                contentFolder, $"{normalizeAssetPath}{normalizeExt}"
            );

            if (File.Exists(candidate))
            {
                foundFullPath = candidate;
                return true;
            }
        }

        foundFullPath = null;
        return false;
    }

    private bool InternalRemove(uint hash, bool removeInDirectory)
    {
        if (!_assets.TryGetValue(hash, out var entry))
            throw new KeyNotFoundException($"No asset found with hash 0x{hash:X8}, unable to remove.");
        if (!entry.Asset.IsValid)
        {
            if (removeInDirectory)
                return _assets.Remove(hash);
            else
                return default;
        }

        TextureAtlasManager.Instance.UnloadAsset(entry.Asset.Handle);

        if (removeInDirectory)
        {
            if (entry.Asset is Sound s)
                s.Dispose();
            else
                entry.Asset.Unload();

            BytesLoaded -= (long)entry.Length;
        }
        else
        {
            entry.Asset.Unload();
            BytesLoaded -= (long)entry.Length;
        }

        if (BytesLoaded < 0) BytesLoaded = 0;

        if (removeInDirectory)
            return _assets.Remove(hash);

        return false;
    }

    private void EvictAssets()
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan evictAfter = TimeSpan.FromMinutes(EvictAfterMinutes);
        var toEvict = new List<uint>();

        foreach (var kvp in _assets)
        {
            TimeSpan age = now - kvp.Value.LastAccessFrame;

            if (age >= evictAfter)
                toEvict.Add(kvp.Key);
        }

        if (toEvict.Count > 0)
        {
            foreach (var key in toEvict)
                InternalRemove(key, false);
        }
    }
    #endregion
}
