namespace System;

/// <summary>
/// Provides extension methods for <see cref="float"/> to simplify layout calculations.
/// </summary>
public static class FloatExtensions
{
	/// <summary>
	/// Calculates the remaining space after subtracting a child size and optional spacing from the parent value.
	/// </summary>
	/// <param name="value">The total available size (width or height).</param>
	/// <param name="childSize">The size of the child element.</param>
	/// <param name="spacing">Optional spacing to subtract in addition to the child size. Defaults to 4.</param>
	/// <returns>
	/// The non-negative remaining space: <c>max(0, value - childSize - spacing)</c>.
	/// </returns>
	public static float RemainingFloat(this float value, float childSize, float spacing = 4) =>
		AlignHelpers.Remaining(value, childSize, spacing);
}

