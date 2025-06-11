using Snap.Assets.Fonts;
using Snap.Helpers;
using Snap.Systems;

namespace System;

public static class FontExtentions
{
	public static Vect2 Measure(this Font font, string t) => font.Measure(t);

	public static float MeasureWidth(this Font font, string t) => Measure(font, t).X;

	public static float MeasureHeight(this Font font, string t) => Measure(font, t).Y;

	public static string FormatText(this Font font, string text, int width) =>
		TextHelpers.FormatText(font, text, width);
}
