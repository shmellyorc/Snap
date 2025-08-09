namespace Snap.Engine.Assets.Spritesheets;

/// <summary>
/// Represents metadata for a single sprite slice within a spritesheet.
/// Includes bounds, optional 9-slice patch region, and pivot point for alignment.
/// </summary>
public class SpritesheetEntry
{
	/// <summary>
	/// The bounding rectangle of the sprite in the spritesheet texture.
	/// Defines the full region of the sprite frame in pixels.
	/// </summary>
	public Rect2 Bounds { get; }

	/// <summary>
	/// The center patch rectangle used for 9-slice scaling, if defined.
	/// A zero patch implies no 9-slice region.
	/// </summary>
	public Rect2 Patch { get; }

	/// <summary>
	/// The pivot point of the sprite relative to its local space, used for alignment or anchoring.
	/// </summary>
	public Vect2 Pivot { get; }

	internal SpritesheetEntry(Rect2 bounds, Rect2 patch, Vect2 pivot)
	{
		Bounds = bounds;
		Patch = patch;
		Pivot = pivot;
	}
}
