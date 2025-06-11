using Snap.Systems;

namespace Snap.Assets.Fonts;
public struct Glyph
{
	// Unicode codepoint (or char) for this glyph
	public char Character;

	// pixel rectangle in the texture (Left, Top, Width, Height)
	public Rect2 Cell;

	// horizontal draw‐offset (e.g. BMFont's xoffset)
	public int XOffset;

	// vertical draw‐offset (e.g. BMFont's yoffset)
	public int YOffset;

	// how far to advance the X cursor after drawing this glyph
	public int Advance;

	// which page texture (0,1,2…) – for a single‐page flood‐fill, always 0
	public int Page;
}
