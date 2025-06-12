using Snap.Systems;

namespace Snap.Paths;

public sealed class Graph
{
	public Dictionary<int, GraphNode> Nodes { get; private set; } = new();

	// Events:

	public event Action<GraphNode> OnNodeAdded;
	public event Action<GraphNode> OnNodeRemoved;
	public event Action<GraphNode> OnNodeEnabled;
	public event Action<GraphNode> OnNodeDisabled;
	public event Action<GraphNode, GraphNode, float> OnEdgeWeightChanged;

	public void AddNode(int id, Vect2 position)
	{
		if (!Nodes.ContainsKey(id))
		{
			GraphNode node = new GraphNode(id, position);
			Nodes[id] = node;
			OnNodeAdded?.Invoke(node);
		}
	}

	public void RemoveNode(int id)
	{
		if (Nodes.ContainsKey(id))
		{
			GraphNode node = Nodes[id];
			Nodes.Remove(id);
			OnNodeRemoved?.Invoke(node);
		}
	}

	public void ConnectNodes(int a, int b, float weight, bool biDirectional = true)
	{
		if (Nodes.ContainsKey(a) && Nodes.ContainsKey(b))
		{
			Nodes[a].Edges[b] = weight;
			if (biDirectional)
				Nodes[b].Edges[a] = weight;
		}
	}

	public void RemoveEdge(int a, int b)
	{
		if (Nodes.ContainsKey(a) && Nodes[a].Edges.ContainsKey(b))
			Nodes[a].Edges.Remove(b);
		if (Nodes.ContainsKey(b) && Nodes[b].Edges.ContainsKey(a))
			Nodes[b].Edges.Remove(a);
	}

	public void UpdateEdgeWeight(int a, int b, float newWeight)
	{
		if (Nodes.ContainsKey(a) && Nodes[a].Edges.ContainsKey(b))
		{
			Nodes[a].Edges[b] = newWeight;
			OnEdgeWeightChanged?.Invoke(Nodes[a], Nodes[b], newWeight);
		}
		if (Nodes.ContainsKey(b) && Nodes[b].Edges.ContainsKey(a))
			Nodes[b].Edges[a] = newWeight;
	}

    public bool HasNode(int id)
    {
		return Nodes.ContainsKey(id);
	}

    public List<int> GetConnectednodes(int id)
    {
		return Nodes.ContainsKey(id) ? new List<int>(Nodes[id].Edges.Keys) : new List<int>();
	}
}
