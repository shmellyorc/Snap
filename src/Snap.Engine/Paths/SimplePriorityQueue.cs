namespace Snap.Engine.Paths;

/// <summary>
/// A priority queue optimized for graph pathfinding algorithms such as Dijkstra or A*.
/// Stores items associated with priority values, allowing fast insertion and retrieval
/// of the item with the lowest priority cost.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item being stored — typically a node, coordinate, or vertex in a graph.
/// </typeparam>
/// <typeparam name="TPriority">
/// The type used for priority comparison — usually <see cref="float"/> or <see cref="int"/>.
/// Lower values indicate higher priority. Must support comparison.
/// </typeparam>
/// <remarks>
/// This queue is intended for use in shortest-path algorithms, where nodes are enqueued
/// based on their estimated total cost or distance. Internal implementation may vary (e.g., heap),
/// but the focus is on minimizing retrieval time for the lowest-cost node.
/// </remarks>
public sealed class GraphPriorityQueue<TItem, TPriority>
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

	private readonly List<Node> _heap = [];
	private readonly Dictionary<TItem, int> _indices = [];

	/// <summary>
	/// Gets the number of items currently stored in the priority queue.
	/// </summary>
	public int Count => _heap.Count;

	/// <summary>
	/// Adds the specified item to the priority queue with the given priority.
	/// </summary>
	/// <param name="item">The item to enqueue. Must not already exist in the queue.</param>
	/// <param name="priority">The priority associated with the item. Lower values indicate higher priority.</param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the item is already present in the queue.
	/// </exception>
	public void Enqueue(TItem item, TPriority priority)
	{
		if (_indices.ContainsKey(item))
			throw new InvalidOperationException("Item is already in the queue");

		int i = _heap.Count;
		_heap.Add(new Node(item, priority));
		_indices[item] = i;

		BubbleUp(i);
	}

	/// <summary>
	/// Removes and returns the item with the lowest priority from the queue.
	/// </summary>
	/// <returns>The item with the lowest priority.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the queue is empty.
	/// </exception>
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

	/// <summary>
	/// Determines whether the specified item is currently in the priority queue.
	/// </summary>
	/// <param name="item">The item to check for existence.</param>
	/// <returns><c>true</c> if the item is in the queue; otherwise, <c>false</c>.</returns>
	public bool Contains(TItem item) => _indices.ContainsKey(item);

	/// <summary>
	/// Updates the priority of an existing item in the queue.
	/// </summary>
	/// <param name="item">The item whose priority should be updated. Must already exist in the queue.</param>
	/// <param name="newPriority">The new priority value to assign. Lower values indicate higher priority.</param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the specified item is not found in the queue.
	/// </exception>
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

	/// <summary>
	/// Removes all items from the queue, resetting it to an empty state.
	/// </summary>
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
