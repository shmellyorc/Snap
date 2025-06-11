namespace Snap.Beacons;

public readonly struct BeaconHandle
{
	public string Topic { get; }
	public object[] Args { get; }

	public bool IsEmpty => Args.IsEmpty();

	internal BeaconHandle(string topic, object[] args)
	{
		Topic = topic;
		Args = args;
	}

	public bool HasArg<T>(int index)
	{
		var element = Args.ElementAtOrDefault(index);

		return element != null && element is T;
	}

	public T GetArg<T>(int index)
	{
        if(!HasArg<T>(index))
			return default;

		var element = Args.ElementAtOrDefault(index);

		return element is T tElement ? tElement : default;
	}

	public bool TryGetArg<T>(int index, out T arg)
	{
		arg = GetArg<T>(index);

		return arg != null;
	}
}
