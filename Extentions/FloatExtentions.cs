namespace System;

public static class FloatExtentions
{
	public static float RemainingFloat(this float value, float childSize, float spacing = 4)
	{
		return AlignHelpers.Remaining(value, childSize, spacing);
	}
}
