namespace Snap.Paths;

public sealed class GraphPathfinder
{
	/// <summary>
	/// Preallocates internal structures for up to <paramref name="capacity"/> nodes.
	/// </summary>
	public void Preallocate(int capacity)
	{
		// Reserve space for nodes
		_adj.Capacity = capacity;
		_indexToId.Capacity = capacity;
		_positions.Capacity = capacity;
		_disabled.Capacity = capacity;

		// Pre-size dictionary if supported
		_idToIndex.EnsureCapacity(capacity);

		// Initialize score arrays
		_gScore = new float[capacity];
		_gVisit = new int[capacity];
	}

	// --- Edge representation (struct for zero-GC) ---
	private struct Edge
	{
		public int To;
		public float Cost;

		public Edge(int to, float cost)
		{
			To = to;
			Cost = cost;
		}
	}

	// --- Core graph storage ---
	private readonly List<List<Edge>> _adj = new();
	private readonly List<int> _indexToId = new();
	private readonly Dictionary<int, int> _idToIndex = new();
	private readonly List<Vect2> _positions = new();
	private readonly List<bool> _disabled = new();

	// --- Timestamped gScore for flow/Dijkstra ---
	private float[] _gScore = Array.Empty<float>();
	private int[] _gVisit = Array.Empty<int>();
	private int _visitMark;
	private readonly GraphPriorityQueue<int, float> _openSet = new();

	// --- Flow-field output & reusable buffers ---
	private int[] _flowNext;
	private readonly List<Vect2> _posBuffer = new List<Vect2>();

	/// <summary>Adds a node with its world-space position.</summary>
	public void AddNode(int id, Vect2 position)
	{
		if (_idToIndex.ContainsKey(id))
			return;
		// throw new InvalidOperationException($"Node {id} already exists.");

		int idx = _indexToId.Count;
		_idToIndex[id] = idx;
		_indexToId.Add(id);
		_adj.Add(new List<Edge>());
		_positions.Add(position);
		_disabled.Add(false);

		Array.Resize(ref _gScore, idx + 1);
		Array.Resize(ref _gVisit, idx + 1);
	}

	/// <summary>Connects two nodes bidirectionally by default.</summary>
	public void ConnectNode(int fromId, int toId, float cost = 1f, bool bidirectional = true)
	{
		if (!_idToIndex.TryGetValue(fromId, out var from) ||
			!_idToIndex.TryGetValue(toId, out var to))
			throw new InvalidOperationException("Both nodes must be added first.");

		var edgesFrom = _adj[from];
		if (!edgesFrom.Exists(e => e.To == to))
			edgesFrom.Add(new Edge(to, cost));

		if (bidirectional)
		{
			var edgesTo = _adj[to];
			if (!edgesTo.Exists(e => e.To == from))
				edgesTo.Add(new Edge(from, cost));
		}
	}

	/// <summary>Checks direct connectivity between two nodes.</summary>
	public bool IsNodeConnected(int fromId, int toId)
	{
		if (!_idToIndex.TryGetValue(fromId, out var from) ||
			!_idToIndex.TryGetValue(toId, out var to))
			return false;

		foreach (var e in _adj[from])
			if (e.To == to) return true;

		return false;
	}

	/// <summary>Disables or enables a node for flow/search.</summary>
	public void DisableNode(int id) => SetDisabled(id, true);
	public void EnableNode(int id) => SetDisabled(id, false);
	public bool IsNodeDisabled(int id) => _idToIndex.TryGetValue(id, out var i) && _disabled[i];
	private void SetDisabled(int id, bool value)
	{
		if (_idToIndex.TryGetValue(id, out var idx))
			_disabled[idx] = value;
	}

	/// <summary>Builds a reverse-Dijkstra flow from a goal node.</summary>
	/// <remarks>Populates _flowNext so each node points to the best neighbor toward goal.</remarks>
	public void ComputeFlowField(int goalId)
	{
		if (!_idToIndex.TryGetValue(goalId, out var goalIdx))
			return;
		// throw new InvalidOperationException("Goal must exist.");

		int n = _indexToId.Count;

		// Build reverse adjacency
		var revAdj = new List<List<Edge>>(n);
		for (int i = 0; i < n; i++) revAdj.Add(new List<Edge>());
		for (int u = 0; u < n; u++)
			foreach (var e in _adj[u])
				revAdj[e.To].Add(new Edge(u, e.Cost));

		// Timestamped Dijkstra from goal
		_visitMark++;
		_openSet.Clear();
		SetG(goalIdx, 0f);
		_openSet.Enqueue(goalIdx, 0f);

		while (_openSet.Count > 0)
		{
			int cur = _openSet.Dequeue();
			float gcur = GetG(cur);

			foreach (var e in revAdj[cur])
			{
				if (_disabled[e.To]) continue;

				float tg = gcur + e.Cost;
				if (tg < GetG(e.To))
				{
					SetG(e.To, tg);
					if (_openSet.Contains(e.To))
						_openSet.UpdatePriority(e.To, tg);
					else
						_openSet.Enqueue(e.To, tg);
				}
			}
		}

		// Populate flowNext
		_flowNext = new int[n];
		for (int u = 0; u < n; u++)
		{
			if (_disabled[u] || float.IsPositiveInfinity(GetG(u)))
			{
				_flowNext[u] = -1;
				continue;
			}

			if (u == goalIdx)
			{
				_flowNext[u] = u;
				continue;
			}

			int best = -1;
			float bestCost = float.PositiveInfinity;

			foreach (var e in _adj[u])
			{
				if (_disabled[e.To]) continue;

				float c = e.Cost + GetG(e.To);

				if (c < bestCost)
				{
					bestCost = c;
					best = e.To;
				}
			}

			_flowNext[u] = best;
		}
	}

	/// <summary>Gets the next node ID along the computed flow.</summary>
	public int GetNextNode(int currentId)
	{
		if (!_idToIndex.TryGetValue(currentId, out var idx) || _flowNext == null)
			return currentId;

		int nextIdx = _flowNext[idx];
		return (nextIdx >= 0 ? _indexToId[nextIdx] : currentId);
	}

	/// <summary>Gets a normalized direction vector along the flow.</summary>
	public Vect2 GetFlowDirection(int currentId)
	{
		int nextId = GetNextNode(currentId);
		if (nextId == currentId) return Vect2.Zero;
		var a = _positions[_idToIndex[currentId]];
		var b = _positions[_idToIndex[nextId]];
		return (b - a).Normalize();
	}

	/// <summary>
	/// Samples the flow-field from start to goal, returning positions along the way.
	/// </summary>
	public List<Vect2> FindPathPositions(int startId, int goalId)
	{
		if (!_idToIndex.TryGetValue(startId, out _) ||
			!_idToIndex.TryGetValue(goalId, out _))
		{
			return new List<Vect2>();
		}

		ComputeFlowField(goalId);

		_posBuffer.Clear();
		int current = startId;
		int goalIdx = _idToIndex[goalId];

		while (true)
		{
			_posBuffer.Add(_positions[_idToIndex[current]]);

			if (_idToIndex[current] == goalIdx)
				break;

			int next = GetNextNode(current);
			if (next == current)
				break; // no further process possible

			current = next;
		}

		return _posBuffer;
	}

	// --- Timestamped gScore helpers ---
	private float GetG(int idx)
		=> (_gVisit[idx] == _visitMark) ? _gScore[idx] : float.PositiveInfinity;

	private void SetG(int idx, float val)
	{
		_gVisit[idx] = _visitMark;
		_gScore[idx] = val;
	}
}
