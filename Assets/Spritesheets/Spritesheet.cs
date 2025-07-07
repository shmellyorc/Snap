namespace Snap.Assets.Spritesheets;

public sealed class Spritesheet : IAsset
{
	public uint Id { get; }
	public string Tag { get; }
	public bool IsValid { get; private set; }
	public uint Handle { get; }

	private readonly Dictionary<uint, SpritesheetEntry> _spritesheets = new();

	internal Spritesheet(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}

	~Spritesheet() => Dispose();

	public ulong Load()
	{
		if (IsValid)
			return 0u;

		if (!File.Exists(Tag))
			throw new FileNotFoundException();

		var bytes = File.ReadAllBytes(Tag);
		var doc = JsonDocument.Parse(bytes);
		var root = doc.RootElement;
		var meta = root.GetProperty("meta");
		var slices = meta.GetProperty("slices");

		foreach (var e in slices.EnumerateArray())
		{
			var name = e.GetPropertyOrDefault<string>("name");
			var key = e.GetProperty("keys").EnumerateArray().First();

			Rect2 rectBounds = Rect2.Zero;
			Rect2 rectPatch = Rect2.Zero;
			Vect2 vectPivot = Vect2.Zero;

			if (key.TryGetProperty("bounds", out var b))
			{
				rectBounds = new Rect2(
					b.GetPropertyOrDefault<int>("x"),
					b.GetPropertyOrDefault<int>("y"),
					b.GetPropertyOrDefault<int>("w"),
					b.GetPropertyOrDefault<int>("h")
				);
			}

			if (key.TryGetProperty("center", out var p))
			{
				rectPatch = new Rect2(
					p.GetPropertyOrDefault<int>("x"),
					p.GetPropertyOrDefault<int>("y"),
					p.GetPropertyOrDefault<int>("w"),
					p.GetPropertyOrDefault<int>("h")
				);
			}

			if (key.TryGetProperty("pivot", out var v))
			{
				vectPivot = new Vect2(
					v.GetPropertyOrDefault<int>("x"),
					v.GetPropertyOrDefault<int>("y")
				);
			}

			_spritesheets[HashHelpers.Hash32(name)] =
				new SpritesheetEntry(rectBounds, rectPatch, vectPivot);
		}

		IsValid = true;

		return (ulong)bytes.Length;
	}

	public void Unload()
	{
		if (!IsValid)
			return;

		Dispose();
	}

	public void Dispose()
	{
		_spritesheets.Clear();
		IsValid = false;

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");
	}

	public Rect2 GetBounds(string name)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Sprite name must not be null or empty.");
		if (!_spritesheets.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new KeyNotFoundException($"No spritesheet entry found for name '{name}'.");
		if (result.Bounds.IsZero)
			throw new InvalidOperationException($"Bounds for spritesheet '{name}' has not been set.");

		return result.Bounds;
	}

	public Rect2 GetPatch(string name)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Sprite name must not be null or empty.");
		if (!_spritesheets.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new KeyNotFoundException($"No spritesheet entry found for name '{name}'.");
		if (result.Patch.IsZero)
			throw new Exception($"9-slice patch for '{name}' has not been defined.");

		return result.Patch;
	}

	public Vect2 GetPivot(string name)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Sprite name must not be null or empty.");
		if (!_spritesheets.TryGetValue(HashHelpers.Hash32(name), out var result))
			throw new KeyNotFoundException($"No spritesheet entry found for name '{name}'.");
		if (result.Pivot.IsZero)
			throw new Exception($"Pivot point for '{name}' has not been set.");

		return result.Pivot;
	}
}
