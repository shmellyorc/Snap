namespace System;

/// <summary>
/// Provides extension methods for <see cref="Font"/> to simplify text measurement and formatting.
/// </summary>
public static class FontExtensions
{
	/// <summary>
	/// Measures the size of the specified text when rendered with this font.
	/// </summary>
	/// <param name="font">The <see cref="Font"/> used for measurement.</param>
	/// <param name="t">The text to measure.</param>
	/// <returns>A <see cref="Vect2"/> where X is the width and Y is the height of the rendered text.</returns>
	public static Vect2 Measure(this Font font, string t) => font.Measure(t);

	/// <summary>
	/// Measures the width of the specified text when rendered with this font.
	/// </summary>
	/// <param name="font">The <see cref="Font"/> used for measurement.</param>
	/// <param name="t">The text to measure.</param>
	/// <returns>The width of the rendered text.</returns>
	public static float MeasureWidth(this Font font, string t) => Measure(font, t).X;

	/// <summary>
	/// Measures the height of the specified text when rendered with this font.
	/// </summary>
	/// <param name="font">The <see cref="Font"/> used for measurement.</param>
	/// <param name="t">The text to measure.</param>
	/// <returns>The height of the rendered text.</returns>
	public static float MeasureHeight(this Font font, string t) => Measure(font, t).Y;

	/// <summary>
	/// Wraps and formats the specified text so that no line exceeds the given width,
	/// using this font's metrics.
	/// </summary>
	/// <param name="font">The <see cref="Font"/> used to measure and wrap the text.</param>
	/// <param name="text">The input text to wrap and format.</param>
	/// <param name="width">The maximum line width in pixels (or font units) allowed before wrapping.</param>
	/// <returns>
	/// A formatted string with line breaks inserted such that each line’s rendered width 
	/// does not exceed <paramref name="width"/>.
	/// </returns>
	public static string FormatText(this Font font, string text, int width) =>
		TextHelpers.FormatText(font, text, width);
}

