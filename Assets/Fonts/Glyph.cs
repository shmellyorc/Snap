namespace Snap.Assets.Fonts;

/// <summary>
/// Represents a single character glyph, including its visual bounds, positioning offsets,
/// advance width, and page index within a bitmap font atlas.
/// </summary>
public struct Glyph
{
	/// <summary>
	/// The Unicode character this glyph represents.
	/// </summary>
	public char Character;

	/// <summary>
	/// The rectangular region of the texture containing the glyph's image.
	/// </summary>
	public Rect2 Cell;

	/// <summary>
	/// The horizontal offset from the baseline where the glyph should be drawn.
	/// </summary>
	public int XOffset;

	/// <summary>
	/// The vertical offset from the baseline where the glyph should be drawn.
	/// </summary>
	public int YOffset;

	/// <summary>
	/// The horizontal advance to apply after rendering this glyph.
	/// </summary>
	public int Advance;

	/// <summary>
	/// The index of the texture page on which this glyph is located.
	/// </summary>
	public int Page;
}
