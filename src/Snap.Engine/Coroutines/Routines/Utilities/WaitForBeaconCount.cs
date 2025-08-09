namespace Snap.Engine.Coroutines.Routines.Utilities;

/// <summary>
/// A coroutine that waits for a topic to be emitted a specified number of times.
/// Optional predicate filters each beacon instance. Supports optional timeout.
/// </summary>
public sealed class WaitForBeaconCount : IEnumerator
{
	private readonly string _topic;
	private readonly int _targetCount;
	private readonly Func<BeaconHandle, bool>? _predicate;
	private readonly float _timeoutSeconds;

	private int _count;
	private bool _subscribed;
	private bool _done;
	private float _elapsed;

	/// <summary>
	/// Always null for <see cref="WaitForBeaconCount"/>.
	/// </summary>
	public object Current => null;

	private readonly Action<BeaconHandle> _handler;

	public WaitForBeaconCount(string topic, int count, Func<BeaconHandle, bool>? predicate = null, float timeoutSeconds = -1f)
	{
		if (string.IsNullOrEmpty(topic))
			throw new ArgumentException("Topic must be non-empty.", nameof(topic));
		if (count <= 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

		_topic = topic;
		_targetCount = count;
		_predicate = predicate;
		_timeoutSeconds = timeoutSeconds;
		_handler = OnBeacon;
	}

	public WaitForBeaconCount(Enum topic, int count, Func<BeaconHandle, bool>? predicate = null, float timeoutSeconds = -1f)
		: this(topic.ToEnumString(), count, predicate, timeoutSeconds) { }

	public bool MoveNext()
	{
		if (_done) return false;

		if (!_subscribed)
		{
			BeaconManager.Instance.Connect(_topic, _handler);
			_subscribed = true;
		}

		if (_timeoutSeconds >= 0f)
		{
			_elapsed += Clock.Instance.DeltaTime;
			if (_elapsed >= _timeoutSeconds)
			{
				Cleanup();
				_done = true;
				return false;
			}
		}

		return true;
	}

	public void Reset() => throw new NotSupportedException();

	private void OnBeacon(BeaconHandle h)
	{
		if (_predicate == null || _predicate(h))
		{
			_count++;
			if (_count >= _targetCount)
			{
				Cleanup();
				_done = true;
			}
		}
	}

	private void Cleanup()
	{
		if (_subscribed)
		{
			try { BeaconManager.Instance.Disconnect(_topic, _handler); }
			catch { /* ignore */ }
			_subscribed = false;
		}
	}
}
