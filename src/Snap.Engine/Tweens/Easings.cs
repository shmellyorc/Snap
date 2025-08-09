namespace Snap.Engine.Tweens;

/// <summary>
/// All supported easing types, combining “family” (Quad, Cubic, etc.) with direction (In, Out, InOut, and OutIn).
/// </summary>
public enum EaseType
{
	// Simple linear (no easing)
	Linear,

	// Quadratic
	QuadIn,
	QuadOut,
	QuadInOut,
	QuadOutIn,

	// Cubic
	CubicIn,
	CubicOut,
	CubicInOut,
	CubicOutIn,

	// Quartic
	QuartIn,
	QuartOut,
	QuartInOut,
	QuartOutIn,

	// Quintic
	QuintIn,
	QuintOut,
	QuintInOut,
	QuintOutIn,

	// Sine
	SineIn,
	SineOut,
	SineInOut,
	SineOutIn,

	// Exponential
	ExpoIn,
	ExpoOut,
	ExpoInOut,
	ExpoOutIn,

	// Circular
	CircIn,
	CircOut,
	CircInOut,
	CircOutIn,

	// Back (overshoot)
	BackIn,
	BackOut,
	BackInOut,
	BackOutIn,

	// Elastic (oscillatory)
	ElasticIn,
	ElasticOut,
	ElasticInOut,
	ElasticOutIn,

	// Bounce (discrete bounces)
	BounceIn,
	BounceOut,
	BounceInOut,
	BounceOutIn,
}

/// <summary>
/// Static class providing a single Ease(...) method that evaluates any of the above easing curves.
/// Input t must be in [0,1]. Output is also in [0,1].
/// </summary>
public static class Easing
{
	/// <summary>
	/// Evaluate the specified easing function at normalized time t ∈ [0,1].
	/// </summary>
	/// <param name="type">Which easing curve to use.</param>
	/// <param name="t">Normalized time, between 0 and 1.</param>
	/// <returns>Eased value, also between 0 and 1.</returns>
	public static float Ease(EaseType type, float t)
	{
		// Clamp t to [0,1]
		t = Math.Clamp(t, 0f, 1f);

		switch (type)
		{
			case EaseType.Linear: return Linear(t);

			case EaseType.QuadIn: return QuadIn(t);
			case EaseType.QuadOut: return QuadOut(t);
			case EaseType.QuadInOut: return QuadInOut(t);
			case EaseType.QuadOutIn: return OutIn(QuadOut, QuadIn, t);

			case EaseType.CubicIn: return CubicIn(t);
			case EaseType.CubicOut: return CubicOut(t);
			case EaseType.CubicInOut: return CubicInOut(t);
			case EaseType.CubicOutIn: return OutIn(CubicOut, CubicIn, t);

			case EaseType.QuartIn: return QuartIn(t);
			case EaseType.QuartOut: return QuartOut(t);
			case EaseType.QuartInOut: return QuartInOut(t);
			case EaseType.QuartOutIn: return OutIn(QuartOut, QuartIn, t);

			case EaseType.QuintIn: return QuintIn(t);
			case EaseType.QuintOut: return QuintOut(t);
			case EaseType.QuintInOut: return QuintInOut(t);
			case EaseType.QuintOutIn: return OutIn(QuintOut, QuintIn, t);

			case EaseType.SineIn: return SineIn(t);
			case EaseType.SineOut: return SineOut(t);
			case EaseType.SineInOut: return SineInOut(t);
			case EaseType.SineOutIn: return OutIn(SineOut, SineIn, t);

			case EaseType.ExpoIn: return ExpoIn(t);
			case EaseType.ExpoOut: return ExpoOut(t);
			case EaseType.ExpoInOut: return ExpoInOut(t);
			case EaseType.ExpoOutIn: return OutIn(ExpoOut, ExpoIn, t);

			case EaseType.CircIn: return CircIn(t);
			case EaseType.CircOut: return CircOut(t);
			case EaseType.CircInOut: return CircInOut(t);
			case EaseType.CircOutIn: return OutIn(CircOut, CircIn, t);

			case EaseType.BackIn: return BackIn(t);
			case EaseType.BackOut: return BackOut(t);
			case EaseType.BackInOut: return BackInOut(t);
			case EaseType.BackOutIn: return OutIn(BackOut, BackIn, t);

			case EaseType.ElasticIn: return ElasticIn(t);
			case EaseType.ElasticOut: return ElasticOut(t);
			case EaseType.ElasticInOut: return ElasticInOut(t);
			case EaseType.ElasticOutIn: return OutIn(ElasticOut, ElasticIn, t);

			case EaseType.BounceIn: return BounceIn(t);
			case EaseType.BounceOut: return BounceOut(t);
			case EaseType.BounceInOut: return BounceInOut(t);
			case EaseType.BounceOutIn: return OutIn(BounceOut, BounceIn, t);

			default:
				return t; // fallback to linear
		}
	}

	private static float OutIn(Func<float, float> easeOut, Func<float, float> easeIn, float t)
	{
		if (t < 0.5f)
			return 0.5f * easeOut(t * 2f);
		else
			return 0.5f * easeIn((t - 0.5f) * 2f) + 0.5f;
	}

	#region Linear
	private static float Linear(float t)
	{
		return t;
	}
	#endregion


	#region Quadratic
	private static float QuadIn(float t)
	{
		return t * t;
	}

	private static float QuadOut(float t)
	{
		return t * (2f - t);
	}

	private static float QuadInOut(float t)
	{
		if (t < 0.5f)
			return 2f * t * t;
		else
			return -1f + (4f - 2f * t) * t;
	}
	#endregion


	#region Cubic
	private static float CubicIn(float t)
	{
		return t * t * t;
	}

	private static float CubicOut(float t)
	{
		float p = t - 1f;
		return p * p * p + 1f;
	}

	private static float CubicInOut(float t)
	{
		if (t < 0.5f)
			return 4f * t * t * t;
		else
		{
			float p = 2f * t - 2f;
			return 0.5f * p * p * p + 1f;
		}
	}
	#endregion


	#region Quartic
	private static float QuartIn(float t)
	{
		return t * t * t * t;
	}

	private static float QuartOut(float t)
	{
		float p = t - 1f;
		return 1f - p * p * p * p;
	}

	private static float QuartInOut(float t)
	{
		if (t < 0.5f)
			return 8f * t * t * t * t;
		else
		{
			float p = t - 1f;
			return 1f - 8f * p * p * p * p;
		}
	}
	#endregion


	#region Quintic
	private static float QuintIn(float t)
	{
		return t * t * t * t * t;
	}

	private static float QuintOut(float t)
	{
		float p = t - 1f;
		return p * p * p * p * p + 1f;
	}

	private static float QuintInOut(float t)
	{
		if (t < 0.5f)
			return 16f * t * t * t * t * t;
		else
		{
			float p = 2f * t - 2f;
			return 0.5f * p * p * p * p * p + 1f;
		}
	}
	#endregion


	#region Sine
	private static float SineIn(float t)
	{
		return 1f - MathF.Cos(t * MathF.PI / 2f);
	}

	private static float SineOut(float t)
	{
		return MathF.Sin(t * MathF.PI / 2f);
	}

	private static float SineInOut(float t)
	{
		return -0.5f * (MathF.Cos(MathF.PI * t) - 1f);
	}
	#endregion


	#region Exponential
	private static float ExpoIn(float t)
	{
		return t == 0f ? 0f : MathF.Pow(2f, 10f * (t - 1f));
	}

	private static float ExpoOut(float t)
	{
		return t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
	}

	private static float ExpoInOut(float t)
	{
		if (t == 0f) return 0f;
		if (t == 1f) return 1f;
		if (t < 0.5f)
			return 0.5f * MathF.Pow(2f, 20f * t - 10f);
		else
			return 1f - 0.5f * MathF.Pow(2f, -20f * t + 10f);
	}
	#endregion


	#region Circular
	private static float CircIn(float t)
	{
		return 1f - MathF.Sqrt(1f - t * t);
	}

	private static float CircOut(float t)
	{
		return MathF.Sqrt(1f - (t - 1f) * (t - 1f));
	}

	private static float CircInOut(float t)
	{
		if (t < 0.5f)
			return 0.5f * (1f - MathF.Sqrt(1f - 4f * t * t));
		else
			return 0.5f * (MathF.Sqrt(1f - (2f * t - 2f) * (2f * t - 2f)) + 1f);
	}
	#endregion


	#region Back (overshoot)
	// Standard overshoot constant
	private const float backS = 1.70158f;

	private static float BackIn(float t)
	{
		return t * t * ((backS + 1f) * t - backS);
	}

	private static float BackOut(float t)
	{
		float p = t - 1f;
		return p * p * ((backS + 1f) * p + backS) + 1f;
	}

	private static float BackInOut(float t)
	{
		float s = backS * 1.525f;
		if (t < 0.5f)
		{
			float p = 2f * t;
			return 0.5f * (p * p * ((s + 1f) * p - s));
		}
		else
		{
			float p = 2f * t - 2f;
			return 0.5f * (p * p * ((s + 1f) * p + s) + 2f);
		}
	}
	#endregion


	#region Elastic (oscillatory)
	private static float ElasticIn(float t)
	{
		if (t == 0f) return 0f;
		if (t == 1f) return 1f;
		const float p = 0.3f;
		float s = p / 4f;
		float invT = t - 1f;
		return -MathF.Pow(2f, 10f * invT) * MathF.Sin((invT - s) * (2f * MathF.PI) / p);
	}

	private static float ElasticOut(float t)
	{
		if (t == 0f) return 0f;
		if (t == 1f) return 1f;
		const float p = 0.3f;
		float s = p / 4f;
		return MathF.Pow(2f, -10f * t) * MathF.Sin((t - s) * (2f * MathF.PI) / p) + 1f;
	}

	private static float ElasticInOut(float t)
	{
		if (t == 0f) return 0f;
		if (t == 1f) return 1f;
		const float p = 0.45f; // slightly longer period for InOut
		float s = p / 4f;
		float invT = 2f * t - 1f;
		if (invT < 0f)
		{
			return -0.5f * MathF.Pow(2f, 10f * invT) * MathF.Sin((invT - s) * (2f * MathF.PI) / p);
		}
		else
		{
			return MathF.Pow(2f, -10f * invT) * MathF.Sin((invT - s) * (2f * MathF.PI) / p) * 0.5f + 1f;
		}
	}
	#endregion


	#region Bounce (piecewise)
	private static float BounceIn(float t)
	{
		return 1f - BounceOut(1f - t);
	}

	private static float BounceOut(float t)
	{
		const float n1 = 7.5625f;
		const float d1 = 2.75f;

		if (t < 1f / d1)
		{
			return n1 * t * t;
		}
		else if (t < 2f / d1)
		{
			float u = t - 1.5f / d1;
			return n1 * u * u + 0.75f;
		}
		else if (t < 2.5f / d1)
		{
			float u = t - 2.25f / d1;
			return n1 * u * u + 0.9375f;
		}
		else
		{
			float u = t - 2.625f / d1;
			return n1 * u * u + 0.984375f;
		}
	}

	private static float BounceInOut(float t)
	{
		if (t < 0.5f)
			return (1f - BounceOut(1f - 2f * t)) * 0.5f;
		else
			return BounceOut(2f * t - 1f) * 0.5f + 0.5f;
	}
	#endregion
}
