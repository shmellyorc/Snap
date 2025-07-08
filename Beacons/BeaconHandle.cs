namespace Snap.Beacons;

/// <summary>
/// A lightweight, immutable struct that represents a published beacon event,
/// containing the topic name and optional arguments passed to subscribers.
/// </summary>
public readonly struct BeaconHandle
{
	/// <summary>
	/// Gets the name of the topic that was emitted.
	/// </summary>
	public string Topic { get; }

	/// <summary>
	/// Gets the array of arguments passed with the beacon.
	/// </summary>
	public object[] Args { get; }

	/// <summary>
	/// Returns <c>true</c> if the beacon has no arguments or the arguments array is empty.
	/// </summary>
	public bool IsEmpty => Args.IsEmpty();

	internal BeaconHandle(string topic, object[] args)
	{
		Topic = topic;
		Args = args;
	}

	/// <summary>
	/// Checks if the argument at the specified index exists and is of the given type.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument.</typeparam>
	/// <param name="index">The index of the argument to check.</param>
	/// <returns><c>true</c> if the argument exists and is of type <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
	public bool HasArg<T>(int index)
	{
		var element = Args.ElementAtOrDefault(index);

		return element != null && element is T;
	}

	/// <summary>
	/// Retrieves the argument at the specified index if it exists and is of the expected type.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument.</typeparam>
	/// <param name="index">The index of the argument to retrieve.</param>
	/// <returns>
	/// The argument cast to <typeparamref name="T"/> if it exists and matches the expected type; 
	/// otherwise, the default value of <typeparamref name="T"/>.
	/// </returns>
	public T GetArg<T>(int index)
	{
		if (!HasArg<T>(index))
			return default;

		var element = Args.ElementAtOrDefault(index);

		return element is T tElement ? tElement : default;
	}

	/// <summary>
	/// Attempts to retrieve the argument at the specified index if it exists and is of the expected type.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument.</typeparam>
	/// <param name="index">The index of the argument to retrieve.</param>
	/// <param name="arg">
	/// When this method returns, contains the argument cast to <typeparamref name="T"/> if successful;
	/// otherwise, the default value of <typeparamref name="T"/>.
	/// </param>
	/// <returns><c>true</c> if the argument exists and is of the correct type; otherwise, <c>false</c>.</returns>
	public bool TryGetArg<T>(int index, out T arg)
	{
		arg = GetArg<T>(index);

		return arg != null;
	}
}
