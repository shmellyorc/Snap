using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Snap.Systems;

namespace Snap.Helpers;

public static class MathHelpers
{
	// <summary>
	/// Linearly interpolates between a and b by t (t ∈ [0,1]).
	/// </summary>
	public static float Lerp(float a, float b, float t)
		=> a + (b - a) * Clamp01(t);

	/// <summary>
	/// Smoothly interpolates between a and b by t using a cubic ease‐in/out curve.
	/// </summary>
	public static float SmoothLerp(float a, float b, float t)
	{
		t = Clamp01(t);
		// smoothstep curve: 3t²−2t³
		float u = t * t * (3f - 2f * t);
		return Lerp(a, b, u);
	}

	public static float LerpPercise(float a, float b, float t) =>
		a + (b - a) * t;

	/// <summary>
	/// Clamps value to [0,1].
	/// </summary>
	public static float Clamp01(float t)
		=> t < 0f ? 0f : (t > 1f ? 1f : t);

	public static float Center(float current, float target, bool rounded)
	{
		return rounded
			? MathF.Round((current - target) / 2f)
			: (current - target) / 2f;
	}

	public static Vect2 To2D(int index, int tilesize)
		=> new(index % tilesize, index / tilesize);

	public static int To1D(Vect2 location, int tilesize)
		=> (int)location.Y * tilesize + (int)location.X;
}
