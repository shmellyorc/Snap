using Snap.Systems;

namespace Snap.Paths;

public sealed class Pathfinding
{
	private Graph _graph;

	public Pathfinding(Graph graph)
	{
		_graph = graph;
	}

	public List<Vect2> FindPathVect2(int start, int goal)
	{
		var path = FindPathInt(start, goal);
		return path.Select(id => _graph.Nodes[id].Position).ToList();
	}

	public List<int> FindPathInt(int start, int goal)
	{
		if (!_graph.HasNode(start) || !_graph.HasNode(goal)) return new List<int>();

		HashSet<int> closedSet = new HashSet<int>();
		Dictionary<int, float> gScore = new Dictionary<int, float> { [start] = 0 };
		Dictionary<int, float> fScore = new Dictionary<int, float> { [start] = Heuristic(start, goal) };
		Dictionary<int, int> cameFrom = new Dictionary<int, int>();

		PriorityQueue<int> openSet = new PriorityQueue<int>();
		openSet.Enqueue(start, fScore[start]);

		while (openSet.Count > 0)
		{
			int current = openSet.Dequeue();

			if (current == goal) return ReconstructPath(cameFrom, current);

			closedSet.Add(current);

			foreach (var neighbour in _graph.Nodes[current].Edges)
			{
				if (!_graph.Nodes[neighbour.Key].Enabled || closedSet.Contains(neighbour.Key)) continue;

				float tenativeGSCore = gScore[current] + neighbour.Value;
				if (!gScore.ContainsKey(neighbour.Key) || tenativeGSCore < gScore[neighbour.Key])
				{
					cameFrom[neighbour.Key] = current;
					gScore[neighbour.Key] = tenativeGSCore;
					fScore[neighbour.Key] = gScore[neighbour.Key] + Heuristic(neighbour.Key, goal);

					if (!openSet.Contans(neighbour.Key))
						openSet.Enqueue(neighbour.Key, fScore[neighbour.Key]);
				}
			}
		}

		return EngineSettings.Instance.AllowPartialPaths ? ReconstructPath(cameFrom, goal) : new List<int>();
	}

	private float Heuristic(int a, int b)
	{
		return (_graph.Nodes[a].Position - _graph.Nodes[b].Position).Length();
	}

	private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int current)
	{
		List<int> path = new List<int> { current };
		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Insert(0, current);
		}
		return path;
	}
}
