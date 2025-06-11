using SFClock = SFML.System.Clock;
using SFTime = SFML.System.Time;

namespace Snap.Systems;

public sealed class Clock
{
	private SFClock _clock;
	private SFTime _time;

	public static Clock Instance { get; private set; }
	public float DeltaTimeRaw => _time.AsSeconds();
	public float DeltaTime => MathF.Min(DeltaTimeRaw, 1f / 30f);

	internal Clock()
	{
		Instance ??= this;

		_clock = new SFClock();
		_time = _clock.Restart();
	}

	internal void Update() => _time = _clock.Restart();
}
