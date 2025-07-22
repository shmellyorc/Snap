namespace Snap.Helpers;

/// <summary>
/// Collection of helpful mathematical utility functions.
/// </summary>
public static class MathHelpers
{
	/// <summary>
	/// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>,
	/// clamping <paramref name="t"/> to the [0,1] range.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor (0 = <paramref name="a"/>, 1 = <paramref name="b"/>).</param>
	/// <returns>The interpolated value.</returns>
	public static float Lerp(float a, float b, float t) =>
		a + (b - a) * Clamp01(t);

	/// <summary>
	/// Performs a smooth (ease‑in‑out) interpolation between <paramref name="a"/> and <paramref name="b"/>.
	/// Uses the smoothstep curve 3t² − 2t³ for a smoother transition.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor (0 = <paramref name="a"/>, 1 = <paramref name="b"/>).</param>
	/// <returns>The smoothly interpolated value.</returns>
	public static float SmoothLerp(float a, float b, float t)
	{
		t = Clamp01(t);
		// smoothstep curve: 3t²−2t³
		float u = t * t * (3f - 2f * t);
		return Lerp(a, b, u);
	}

	/// <summary>
	/// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>
	/// without clamping.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor.</param>
	/// <returns>The interpolated value.</returns>
	public static float LerpPercise(float a, float b, float t) =>
		a + (b - a) * t;

	/// <summary>
	/// Clamps the given value to the [0,1] range.
	/// </summary>
	/// <param name="t">The value to clamp.</param>
	/// <returns><c>0</c> if <paramref name="t"/> is less than 0; <c>1</c> if greater than 1; otherwise <paramref name="t"/>.</returns>
	public static float Clamp01(float t) =>
		t < 0f ? 0f : (t > 1f ? 1f : t);

	/// <summary>
	/// Calculates the midpoint between <paramref name="current"/> and <paramref name="target"/>.
	/// Optionally rounds the result.
	/// </summary>
	/// <param name="current">The current value.</param>
	/// <param name="target">The target value.</param>
	/// <param name="rounded">Whether to round the midpoint to the nearest integer.</param>
	/// <returns>The midpoint between <paramref name="current"/> and <paramref name="target"/>.</returns>
	public static float Center(float current, float target, bool rounded)
	{
		return rounded
			? MathF.Round((current - target) / 2f)
			: (current - target) / 2f;
	}

	/// <summary>
	/// Converts a 1‑dimensional tile index into a 2D coordinate.
	/// </summary>
	/// <param name="index">The flat index.</param>
	/// <param name="tilesize">The width (and height) of the tile grid.</param>
	/// <returns>A <see cref="Vect2"/> representing the (x, y) tile position.</returns>
	public static Vect2 To2D(int index, int tilesize) =>
		new(index % tilesize, index / tilesize);

	/// <summary>
	/// Converts a 2D tile coordinate into a 1‑dimensional index.
	/// </summary>
	/// <param name="location">The (x, y) tile position.</param>
	/// <param name="tilesize">The width (and height) of the tile grid.</param>
	/// <returns>The flat index corresponding to <paramref name="location"/>.</returns>
	public static int To1D(Vect2 location, int tilesize) =>
		(int)location.Y * tilesize + (int)location.X;

	/// <summary>
	/// Wraps an integer value into the range [<paramref name="min"/>, <paramref name="max"/>).
	/// </summary>
	/// <param name="value">The value to wrap.</param>
	/// <param name="min">The inclusive lower bound.</param>
	/// <param name="max">The exclusive upper bound. Must be greater than <paramref name="min"/>.</param>
	/// <returns>The wrapped value within [<paramref name="min"/>, <paramref name="max"/>).</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
	public static int Wrap(int value, int min, int max)
	{
		int range = max - min;

		if (range <= 0)
			throw new ArgumentException("max must be greater than min");

		int mod = (value - min) % range;

		if (min < 0)
			mod += range;

		return mod + min;
	}

	/// <summary>
	/// Wraps a floating‑point value into the range [<paramref name="min"/>, <paramref name="max"/>).
	/// </summary>
	/// <param name="value">The value to wrap.</param>
	/// <param name="min">The inclusive lower bound.</param>
	/// <param name="max">The exclusive upper bound. Must be greater than <paramref name="min"/>.</param>
	/// <returns>The wrapped value within [<paramref name="min"/>, <paramref name="max"/>).</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
	public static float Wrap(float value, float min, float max)
	{
		float range = max - min;

		if (range <= 0f)
			throw new ArgumentException("max must be greater than min");

		float mod = (value - min) % range;

		if (mod < 0f)
			mod += range;

		return mod + min;
	}
}
