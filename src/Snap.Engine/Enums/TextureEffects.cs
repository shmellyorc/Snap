namespace Snap.Engine.Enums;

/// <summary>
/// Specifies texture rendering effects that can be combined to flip textures horizontally and/or vertically.
/// </summary>
[Flags]
public enum TextureEffects
{
	/// <summary>
	/// No effect; the texture is rendered normally.
	/// </summary>
	None = 0,

	/// <summary>
	/// Flip the texture horizontally (mirror on the vertical axis).
	/// </summary>
	FlipHorizontal = 1 << 0,

	/// <summary>
	/// Flip the texture vertically (mirror on the horizontal axis).
	/// </summary>
	FlipVertical = 1 << 1,
}

