namespace System;

public static class EnumExtensions
{
	public static float MeasureWidth(this Enum value, string text)
	{
		var f = AssetManager.GetFont(value);
		return f.MeasureWidth(text);
	}

	public static float MeasureHeight(this Enum value, string text)
	{
		var f = AssetManager.GetFont(value);
		return f.MeasureHeight(text);
	}

	public static Vect2 Measure(this Enum value, string text)
	{
		var f = AssetManager.GetFont(value);
		return f.Measure(text);
	}
}