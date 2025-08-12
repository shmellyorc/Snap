namespace Snap.Engine.Enums;

/// <summary>
/// All supported easing types, combining easing family 
/// (e.g., Quadratic, Cubic, Sine) with direction (In, Out, InOut, OutIn).
/// </summary>
/// <remarks>
/// <para>
/// Easing functions control the rate of change of a value over time, producing 
/// different acceleration and deceleration patterns for animations.
/// </para>
/// <para>
/// <b>Directions:</b>
/// <list type="bullet">
/// <item><description><c>In</c> – Starts slow, accelerates towards the end.</description></item>
/// <item><description><c>Out</c> – Starts fast, decelerates towards the end.</description></item>
/// <item><description><c>InOut</c> – Starts slow, accelerates, then slows again.</description></item>
/// <item><description><c>OutIn</c> – Starts fast, slows in the middle, then accelerates again.</description></item>
/// </list>
/// </para>
/// </remarks>
public enum EaseType
{
	/// <summary>No easing. Linear interpolation.</summary>
	Linear,

	// Quadratic
	/// <summary>Quadratic ease-in.</summary>
	QuadIn,
	/// <summary>Quadratic ease-out.</summary>
	QuadOut,
	/// <summary>Quadratic ease-in-out.</summary>
	QuadInOut,
	/// <summary>Quadratic ease-out-in.</summary>
	QuadOutIn,

	// Cubic
	/// <summary>Cubic ease-in.</summary>
	CubicIn,
	/// <summary>Cubic ease-out.</summary>
	CubicOut,
	/// <summary>Cubic ease-in-out.</summary>
	CubicInOut,
	/// <summary>Cubic ease-out-in.</summary>
	CubicOutIn,

	// Quartic
	/// <summary>Quartic ease-in.</summary>
	QuartIn,
	/// <summary>Quartic ease-out.</summary>
	QuartOut,
	/// <summary>Quartic ease-in-out.</summary>
	QuartInOut,
	/// <summary>Quartic ease-out-in.</summary>
	QuartOutIn,

	// Quintic
	/// <summary>Quintic ease-in.</summary>
	QuintIn,
	/// <summary>Quintic ease-out.</summary>
	QuintOut,
	/// <summary>Quintic ease-in-out.</summary>
	QuintInOut,
	/// <summary>Quintic ease-out-in.</summary>
	QuintOutIn,

	// Sine
	/// <summary>Sine ease-in.</summary>
	SineIn,
	/// <summary>Sine ease-out.</summary>
	SineOut,
	/// <summary>Sine ease-in-out.</summary>
	SineInOut,
	/// <summary>Sine ease-out-in.</summary>
	SineOutIn,

	// Exponential
	/// <summary>Exponential ease-in.</summary>
	ExpoIn,
	/// <summary>Exponential ease-out.</summary>
	ExpoOut,
	/// <summary>Exponential ease-in-out.</summary>
	ExpoInOut,
	/// <summary>Exponential ease-out-in.</summary>
	ExpoOutIn,

	// Circular
	/// <summary>Circular ease-in.</summary>
	CircIn,
	/// <summary>Circular ease-out.</summary>
	CircOut,
	/// <summary>Circular ease-in-out.</summary>
	CircInOut,
	/// <summary>Circular ease-out-in.</summary>
	CircOutIn,

	// Back (overshoot)
	/// <summary>Back ease-in (overshoots at start).</summary>
	BackIn,
	/// <summary>Back ease-out (overshoots at end).</summary>
	BackOut,
	/// <summary>Back ease-in-out (overshoots at both ends).</summary>
	BackInOut,
	/// <summary>Back ease-out-in (overshoots in middle).</summary>
	BackOutIn,

	// Elastic (oscillatory)
	/// <summary>Elastic ease-in (spring effect at start).</summary>
	ElasticIn,
	/// <summary>Elastic ease-out (spring effect at end).</summary>
	ElasticOut,
	/// <summary>Elastic ease-in-out (spring effect at both ends).</summary>
	ElasticInOut,
	/// <summary>Elastic ease-out-in (spring effect in middle).</summary>
	ElasticOutIn,

	// Bounce (discrete bounces)
	/// <summary>Bounce ease-in.</summary>
	BounceIn,
	/// <summary>Bounce ease-out.</summary>
	BounceOut,
	/// <summary>Bounce ease-in-out.</summary>
	BounceInOut,
	/// <summary>Bounce ease-out-in.</summary>
	BounceOutIn,
}
