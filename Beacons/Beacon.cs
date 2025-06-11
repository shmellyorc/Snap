using System.Collections;
using System.ComponentModel;

using Coroutines;

using Snap.Coroutines.Routines.Time;
using Snap.Helpers;
using Snap.Logs;

namespace Snap.Beacons;

/// <summary>
/// A high-performance publish/subscribe (Beacon) System
/// </summary>
public sealed class BeaconManager
{
	private class Subscription
	{
		public object Owner;
		public Action<BeaconHandle> Handler;
	}

	// Internally container mapping each topic to its list of subscribers + a lock object.
	// We store a small "Holder" object so reach topic has its own lock.
	private class TopicEntry
	{
		public readonly object SyncRoot = new object();
		public List<Subscription> Subscribers = new();
	}

	/*
        Provides:
            1. O(1) Lookup on the TopEntry (just a directory lookup)
            2. O(n_subscribers) to create a snapshot copy of the subscribers list and to invoke 
               each callback
            3. Per-Topic locking, so different topics don't serialze on one global lock.
            4. Safe handling of subcribers/unsubscribers even while a publish is in progress (no
               InvalidOperationException from modifying a collection during emnumeration)
    */

	private static readonly object _publicOwner = new();
	private readonly Dictionary<uint, TopicEntry> _topics = new();
	private readonly object _mapLock = new();

	public static BeaconManager Instance { get; private set; }
	public int Count => _topics.Count;

	internal BeaconManager() => Instance ??= this;

	public void Connect(Enum topic, Action<BeaconHandle> handler) =>
		Connect(topic.ToEnumString(), _publicOwner, handler);
	public void Connect(string topic, Action<BeaconHandle> handler) =>
		Connect(topic, _publicOwner, handler);
	internal void Connect(string topic, object owner, Action<BeaconHandle> handler)
	{
		if (string.IsNullOrEmpty(topic))
			throw new ArgumentException("Topic must be a non-empty string", nameof(topic));
		if (handler == null) 
			throw new ArgumentNullException(nameof(handler));

		uint hash = HashHelpers.Hash32(topic);
		TopicEntry entry;
		lock (_mapLock)
		{
			if (!_topics.TryGetValue(hash, out entry))
			{
				entry = new TopicEntry();
				_topics[hash] = entry;
			}
		}

		lock (entry.SyncRoot)
		{
			entry.Subscribers.Add(new Subscription
			{
				Owner = owner,
				Handler = handler
			});
		}
	}

	public void Disconnect(string topic, Action<BeaconHandle> handler) =>
		Disconnect(topic, _publicOwner, handler);
	public void Disconnect(Enum topic, Action<BeaconHandle> handler) =>
		Disconnect(topic.ToEnumString(), _publicOwner, handler);
	internal void Disconnect(string topic, object owner, Action<BeaconHandle> handler)
	{
		if (string.IsNullOrEmpty(topic) || handler == null) return;

		TopicEntry entry;
		uint hash = HashHelpers.Hash32(topic);

		lock (_mapLock)
			if (!_topics.TryGetValue(hash, out entry)) return; // No such topic, nothing to do.

		lock (entry.SyncRoot)
		{
			entry.Subscribers.RemoveAll(s => ReferenceEquals(s.Owner, owner)
				&& s.Handler == handler);

			//If no more subscribers, clea up topic entry
			if (entry.Subscribers.Count == 0)
			{
				lock (_mapLock)
				{
					_topics.Remove(hash);
				}
			}
		}
	}

	public void EmitDelayed(Enum topic, float seconds, params object[] args) =>
		EmitDelayed(_publicOwner, topic.ToEnumString(), seconds, args);
	public void EmitDelayed(string topic, float seconds, params object[] args) =>
		EmitDelayed(_publicOwner, topic, seconds, args);
	internal void EmitDelayed(object owner, string topic, float seconds, params object[] args)
	{
		IEnumerator Delayed(float seconds)
		{
			yield return new WaitForSeconds(seconds);

			Emit(topic, args);
		}

		CoroutineManager.Instance.Start(Delayed(seconds), owner);
	}

	public void Emit(Enum topic, params object[] args) =>
		Emit(topic.ToEnumString(), args);
	public void Emit(string topic, params object[] args)
	{
		if (string.IsNullOrEmpty(topic))
			throw new ArgumentException("Topic must be a non-empty string", nameof(topic));

		TopicEntry entry;
		uint hash = HashHelpers.Hash32(topic);

		lock (_mapLock)
		{
			if (!_topics.TryGetValue(hash, out entry))
			{
				// No subsribers for this topic => nothing to do.
				return;
			}
		}

		List<Subscription> subscribersSnapshot;
		lock (entry.SyncRoot)
		{
			subscribersSnapshot = new List<Subscription>(entry.Subscribers);
		}

		var handle = new BeaconHandle(topic, (args == null || args.Length == 0)
			? Array.Empty<object>() : args);

		foreach (var sub in subscribersSnapshot)
		{
			sub.Handler.Invoke(handle);

			// try
			// {
				
			// }
			// catch (Exception ex)
			// {
			// 	// throw new Exception($"{ex}");
			// }
		}
	}

	internal void ClearOwner(object owner)
	{
		if (owner == null) throw new ArgumentNullException(nameof(owner));

		lock (_mapLock)
		{
			foreach (var kv in _topics.ToList())
			{
				var entry = kv.Value;
				lock (entry.SyncRoot)
				{
					entry.Subscribers.RemoveAll(s => ReferenceEquals(s.Owner, owner));
					if (entry.Subscribers.Count == 0)
						_topics.Remove(kv.Key);
				}
			}
		}
	}

	internal void ClearPublicSubscriptions() => ClearOwner(_publicOwner);
}
