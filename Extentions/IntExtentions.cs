using Snap.Helpers;

namespace System;

public static class IntExtentions
{
	public static float RemainingInt(this int value, int childSize, int spacing = 4)
	{
		return AlignHelpers.Remaining(value, childSize, spacing);
	}
}
