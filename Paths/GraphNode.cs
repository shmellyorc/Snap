using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Snap.Systems;

namespace Snap.Paths;

public sealed class GraphNode
{
    public int ID { get; private set; }
    public Vect2 Position { get; private set; }
	public Dictionary<int, float> Edges { get; private set; } = new();
	public bool Enabled { get; private set; } = true;

    public GraphNode(int id, Vect2 position)
    {
		ID = id;
		Position = position;
	}

    public void SetEnabled(bool state)
    {
		Enabled = state;
	}
}
