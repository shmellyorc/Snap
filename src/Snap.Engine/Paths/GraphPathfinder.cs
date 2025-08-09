namespace Snap.Engine.Paths;

/// <summary>
/// Provides graph pathfinding functionality using algorithms such as Dijkstra or A*.
/// Operates on graph nodes and edges to compute the shortest or optimal path between points,
/// using customizable cost and heuristic functions.
/// </summary>
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

	/// <summary>
	/// Adds a new node to the pathfinding graph with a unique ID and world-space position.
	/// </summary>
	/// <param name="id">A unique integer ID representing the node. Duplicate IDs are ignored.</param>
	/// <param name="position">The world-space position of the node, used for distance and heuristic calculations.</param>
	/// <remarks>
	/// If the node ID already exists in the graph, the call is ignored silently.
	/// Internally, this method initializes adjacency lists, position tracking, and score buffers
	/// needed for pathfinding algorithms.
	/// </remarks>
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

	/// <summary>
	/// Creates a connection (edge) between two nodes in the graph, optionally bidirectional.
	/// </summary>
	/// <param name="fromId">The ID of the source node.</param>
	/// <param name="toId">The ID of the destination node.</param>
	/// <param name="cost">The traversal cost associated with the edge. Defaults to <c>1.0</c>.</param>
	/// <param name="bidirectional">
	/// If <c>true</c> (default), a reverse connection from <paramref name="toId"/> to <paramref name="fromId"/> is also created.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if either node has not been added to the graph using <see cref="AddNode"/>.
	/// </exception>
	/// <remarks>
	/// Duplicate edges between the same two nodes are ignored.
	/// </remarks>
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

	/// <summary>
	/// Determines whether a direct edge exists from one node to another.
	/// </summary>
	/// <param name="fromId">The ID of the source node.</param>
	/// <param name="toId">The ID of the destination node.</param>
	/// <returns>
	/// <c>true</c> if a direct connection (edge) exists from <paramref name="fromId"/> to <paramref name="toId"/>; otherwise, <c>false</c>.
	/// </returns>
	/// <remarks>
	/// This only checks one-way connectivity. If the edge is bidirectional, both directions must be checked separately.
	/// Returns <c>false</c> if either node ID is not present in the graph.
	/// </remarks>
	public bool IsNodeConnected(int fromId, int toId)
	{
		if (!_idToIndex.TryGetValue(fromId, out var from) ||
			!_idToIndex.TryGetValue(toId, out var to))
			return false;

		foreach (var e in _adj[from])
			if (e.To == to) return true;

		return false;
	}

	/// <summary>
	/// Marks a node as disabled, excluding it from pathfinding and search operations.
	/// </summary>
	/// <param name="id">The ID of the node to disable.</param>
	/// <remarks>
	/// Disabled nodes are treated as unreachable during graph traversal.
	/// </remarks>
	public void DisableNode(int id) => SetDisabled(id, true);

	/// <summary>
	/// Re-enables a previously disabled node, allowing it to be included in pathfinding and search.
	/// </summary>
	/// <param name="id">The ID of the node to enable.</param>
	public void EnableNode(int id) => SetDisabled(id, false);

	/// <summary>
	/// Checks whether the specified node is currently disabled.
	/// </summary>
	/// <param name="id">The ID of the node to check.</param>
	/// <returns><c>true</c> if the node is disabled; otherwise, <c>false</c>.</returns>
	public bool IsNodeDisabled(int id) => _idToIndex.TryGetValue(id, out var i) && _disabled[i];

	/// <summary>
	/// Internal helper to mark a node as enabled or disabled.
	/// </summary>
	/// <param name="id">The ID of the node.</param>
	/// <param name="value"><c>true</c> to disable the node; <c>false</c> to enable it.</param>
	private void SetDisabled(int id, bool value)
	{
		if (_idToIndex.TryGetValue(id, out var idx))
			_disabled[idx] = value;
	}

	/// <summary>
	/// Computes a reverse Dijkstra-based flow field from the specified goal node.
	/// </summary>
	/// <param name="goalId">The ID of the goal node to build the flow field from.</param>
	/// <remarks>
	/// This method builds a flow field by computing the shortest distance from all nodes
	/// to the goal using Dijkstra’s algorithm in reverse (traversing edges backward).
	/// <para/>
	/// The result is stored in <c>_flowNext</c>, where each node maps to the neighbor
	/// that brings it closest to the goal along the shortest path.
	/// <para/>
	/// Disabled nodes are ignored during path calculation. Nodes unreachable from the goal
	/// will have <c>_flowNext[n] = -1</c>.
	/// </remarks>
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

	/// <summary>
	/// Retrieves the next node ID to move toward the goal from the specified current node,
	/// based on the most recently computed flow field.
	/// </summary>
	/// <param name="currentId">The ID of the current node.</param>
	/// <returns>
	/// The ID of the next node along the shortest path to the goal.  
	/// If the flow field has not been computed or no valid next step exists,
	/// returns <paramref name="currentId"/>.
	/// </returns>
	/// <remarks>
	/// This method uses the <c>_flowNext</c> array populated by <see cref="ComputeFlowField"/>.
	/// If the current node is unreachable or not part of the graph, no movement occurs.
	/// </remarks>
	public int GetNextNode(int currentId)
	{
		if (!_idToIndex.TryGetValue(currentId, out var idx) || _flowNext == null)
			return currentId;

		int nextIdx = _flowNext[idx];
		return nextIdx >= 0 ? _indexToId[nextIdx] : currentId;
	}

	/// <summary>
	/// Computes a normalized direction vector pointing from the current node
	/// toward the next node in the flow field.
	/// </summary>
	/// <param name="currentId">The ID of the current node.</param>
	/// <returns>
	/// A unit-length <see cref="Vect2"/> direction vector pointing toward the goal.
	/// Returns <see cref="Vect2.Zero"/> if the current node has no valid flow direction
	/// or is the goal itself.
	/// </returns>
	/// <remarks>
	/// This method uses world-space positions and the flow field generated by <see cref="ComputeFlowField"/>.
	/// </remarks>
	public Vect2 GetFlowDirection(int currentId)
	{
		int nextId = GetNextNode(currentId);
		if (nextId == currentId) return Vect2.Zero;
		var a = _positions[_idToIndex[currentId]];
		var b = _positions[_idToIndex[nextId]];
		return (b - a).Normalize();
	}

	/// <summary>
	/// Computes a path from the start node to the goal node by following the flow field,
	/// and returns a list of world-space positions along that path.
	/// </summary>
	/// <param name="startId">The ID of the starting node.</param>
	/// <param name="goalId">The ID of the goal node.</param>
	/// <returns>
	/// A list of <see cref="Vect2"/> positions representing the sampled path through the graph.
	/// If no path exists, the list will contain only the start position or be empty.
	/// </returns>
	/// <remarks>
	/// This method computes the flow field from the goal, then walks forward from the start node
	/// using <see cref="GetNextNode"/> to follow the path. Each node’s position is recorded along the way.
	/// </remarks>
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
		=> _gVisit[idx] == _visitMark ? _gScore[idx] : float.PositiveInfinity;

	private void SetG(int idx, float val)
	{
		_gVisit[idx] = _visitMark;
		_gScore[idx] = val;
	}
}
