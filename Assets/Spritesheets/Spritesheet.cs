namespace Snap.Assets.Spritesheets;

/// <summary>
/// Represents a collection of sprite metadata parsed from a JSON spritesheet definition.
/// Provides lookup access to bounds, pivot, and 9-slice patch regions for named sprites.
/// </summary>
public sealed class Spritesheet : IAsset
{
	/// <inheritdoc/>
	public uint Id { get; }

	/// <inheritdoc/>
	public string Tag { get; }

	/// <inheritdoc/>
	public bool IsValid { get; private set; }

	/// <inheritdoc/>
	public uint Handle { get; }

	private readonly Dictionary<uint, SpritesheetEntry> _spritesheets = [];

	internal Spritesheet(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}

	/// <summary>
	/// Destructor that ensures the asset is disposed if not already unloaded.
	/// </summary>
	~Spritesheet() => Dispose();

	/// <summary>
	/// Loads sprite definitions from the metadata file specified by <see cref="Tag"/>.
	/// Parses pivot, bounds, and 9-slice information into internal lookup structures.
	/// </summary>
	/// <returns>The number of bytes read from disk.</returns>
	/// <exception cref="FileNotFoundException">
	/// Thrown if the metadata file does not exist.
	/// </exception>
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

	/// <summary>
	/// Unloads the asset and clears all parsed sprite metadata.
	/// </summary>
	public void Unload()
	{
		if (!IsValid)
			return;

		Dispose();
	}

	/// <summary>
	/// Releases all internal resources and unregisters any managed metadata.
	/// </summary>
	public void Dispose()
	{
		_spritesheets.Clear();
		IsValid = false;

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");
	}

	/// <summary>
	/// Retrieves the bounding rectangle of a sprite by name.
	/// </summary>
	/// <param name="name">The name of the sprite.</param>
	/// <returns>The <see cref="Rect2"/> representing the sprite’s bounds within the sheet.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null or empty.</exception>
	/// <exception cref="KeyNotFoundException">Thrown if no sprite is found with the given name.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the bounds are not defined.</exception>
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

	/// <summary>
	/// Retrieves the 9-slice patch rectangle (center region) of a sprite by name.
	/// </summary>
	/// <param name="name">The sprite’s name in the metadata file.</param>
	/// <returns>The center patch region as a <see cref="Rect2"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null or empty.</exception>
	/// <exception cref="KeyNotFoundException">Thrown if no entry is found for the specified name.</exception>
	/// <exception cref="Exception">Thrown if the patch region is not defined for that sprite.</exception>
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

	/// <summary>
	/// Retrieves the pivot point of the sprite for alignment or transformation.
	/// </summary>
	/// <param name="name">The name of the sprite within the sheet.</param>
	/// <returns>The pivot position as a <see cref="Vect2"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null or empty.</exception>
	/// <exception cref="KeyNotFoundException">Thrown if no entry is found for the name provided.</exception>
	/// <exception cref="Exception">Thrown if the pivot is unset.</exception>
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
