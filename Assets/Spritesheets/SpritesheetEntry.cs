namespace Snap.Assets.Spritesheets;

public class SpritesheetEntry
{
	public Rect2 Bounds { get; }
	public Rect2 Patch { get; }
	public Vect2 Pivot { get; }

	public SpritesheetEntry(Rect2 bounds, Rect2 patch, Vect2 pivot)
	{
		Bounds = bounds;
		Patch = patch;
		Pivot = pivot;
	}
}
