namespace System;

/// <summary>
/// Provides extension methods for integer types related to layout and alignment calculations.
/// </summary>
public static class IntExtentions
{
	/// <summary>
	/// Calculates the remaining space after fitting as many child elements as possible,
	/// given the total size, the size of each child, and optional spacing between them.
	/// </summary>
	/// <param name="value">The total available size (e.g., width or height).</param>
	/// <param name="childSize">The size of a single child element.</param>
	/// <param name="spacing">The spacing between child elements. Defaults to 4.</param>
	/// <returns>
	/// The remaining unused space after placing child elements with the given size and spacing.
	/// </returns>
	public static float RemainingInt(this int value, int childSize, int spacing = 4)
	{
		return AlignHelpers.Remaining(value, childSize, spacing);
	}
}
