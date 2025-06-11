namespace Snap.Inputs;

public class InputMapEntry
{
	private readonly object _value;

	public T ValueAs<T>() where T : Enum => (T)_value;

	internal InputMapEntry(object value) => _value = value;
}
