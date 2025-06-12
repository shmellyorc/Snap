namespace Snap.Paths;

public class GraphPriorityQueue<TItem, TPriority>
	where TItem : notnull
	where TPriority : IComparable<TPriority>
{
	private struct Node
	{
		public TItem Item;
		public TPriority Priority;
		public Node(TItem item, TPriority priority)
		{
			Item = item;
			Priority = priority;
		}
	}

	private readonly List<Node> _heap = new();
	private readonly Dictionary<TItem, int> _indices = new();

	public int Count => _heap.Count;

	public void Enqueue(TItem item, TPriority priority)
	{
		if (_indices.ContainsKey(item))
			throw new InvalidOperationException("Item is already in the queue");
		int i = _heap.Count;
		_heap.Add(new Node(item, priority));
		_indices[item] = i;
		BubbleUp(i);
	}

	public TItem Dequeue()
	{
		if (_heap.Count == 0)
			throw new InvalidOperationException("Queue is empty");
		var root = _heap[0].Item;

		// Move last node to root
		var last = _heap[^1];
		_heap[0] = last;
		_indices[last.Item] = 0;

		_heap.RemoveAt(_heap.Count - 1);
		_indices.Remove(root);

		if (_heap.Count > 0)
			BubbleDown(0);

		return root;
	}

	public bool Contains(TItem item) => _indices.ContainsKey(item);

	public void UpdatePriority(TItem item, TPriority newPriority)
	{
		if (!_indices.TryGetValue(item, out int i))
			throw new InvalidOperationException("Item not found in the queue");

		// extract, modify, and write back
		var node = _heap[i];
		var old = node.Priority;
		node.Priority = newPriority;
		_heap[i] = node;

		// then bubble
		if (newPriority.CompareTo(old) < 0)
			BubbleUp(i);
		else
			BubbleDown(i);
	}

	public void Clear()
	{
		_heap.Clear();
		_indices.Clear();
	}

	private void BubbleUp(int i)
	{
		while (i > 0)
		{
			int parent = (i - 1) / 2;
			if (_heap[i].Priority.CompareTo(_heap[parent].Priority) >= 0)
				break;
			Swap(i, parent);
			i = parent;
		}
	}

	private void BubbleDown(int i)
	{
		int n = _heap.Count;
		while (true)
		{
			int left = 2 * i + 1, right = left + 1, smallest = i;
			if (left < n && _heap[left].Priority.CompareTo(_heap[smallest].Priority) < 0)
				smallest = left;
			if (right < n && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0)
				smallest = right;
			if (smallest == i) break;
			Swap(i, smallest);
			i = smallest;
		}
	}

	private void Swap(int i, int j)
	{
		var tmp = _heap[i];
		_heap[i] = _heap[j];
		_heap[j] = tmp;

		_indices[_heap[i].Item] = i;
		_indices[_heap[j].Item] = j;
	}
}
