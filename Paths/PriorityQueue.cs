namespace Snap.Paths;

public class PriorityQueue<T>
{
	private List<(T item, float priority)> _elements = new();

	public int Count => _elements.Count;

	public void Enqueue(T item, float priority)
	{
		_elements.Add((item, priority));
		_elements = _elements.OrderBy(x => x.priority).ToList();
	}

	public T Dequeue()
	{
		if (_elements.Count == 0) return default;
		var item = _elements[0].item;
		_elements.RemoveAt(0);
		return item;
	}

	public bool Contans(T item)
	{
		return _elements.Any(x => EqualityComparer<T>.Default.Equals(x.item, item));
	}
}
