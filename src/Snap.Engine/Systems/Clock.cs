namespace Snap.Engine.Systems;

/// <summary>
/// Provides a singleton clock for tracking frame time and delta time.
/// </summary>
public sealed class Clock
{
	private readonly SFClock _clock;
	private SFTime _time;

	/// <summary>
	/// Gets the singleton instance of the <see cref="Clock"/>.
	/// </summary>
	public static Clock Instance { get; private set; }

	/// <summary>
	/// Gets the raw delta time (in seconds) since the last update.
	/// </summary>
	/// <remarks>
	/// This value is not clamped and represents the actual elapsed time.
	/// </remarks>
	public float DeltaTimeRaw => _time.AsSeconds();

	/// <summary>
	/// Gets the clamped delta time (in seconds) since the last update.
	/// </summary>
	/// <remarks>
	/// The returned value is capped at <c>1/30</c> of a second to prevent excessively large time steps.
	/// </remarks>
	public float DeltaTime => MathF.Min(DeltaTimeRaw, 1f / 30f);

	internal Clock()
	{
		Instance ??= this;

		_clock = new SFClock();
		_time = _clock.Restart();
	}

	internal void Update() => _time = _clock.Restart();
}
