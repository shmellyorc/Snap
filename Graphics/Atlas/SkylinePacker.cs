namespace Snap.Graphics.Atlas;

public class SkylinePacker
{
	/// <summary>
	/// A single skyline node at X with a given Width and current top Y.
	/// </summary>
	private class Node
	{
		public int X, Y, Width;
		public Node(int x, int y, int width) { X = x; Y = y; Width = width; }
	}

	private readonly int _pageWidth;
	private readonly int _pageHeight;
	private readonly List<Node> _nodes;

	public SkylinePacker(int pageWidth, int pageHeight)
	{
		_pageWidth = pageWidth;
		_pageHeight = pageHeight;
		_nodes = new List<Node> { new Node(0, 0, pageWidth) };
	}

	/// <summary>
	/// Completely reset the packer to one empty skyline.
	/// </summary>
	public void Reset()
	{
		_nodes.Clear();
		_nodes.Add(new Node(0, 0, _pageWidth));
	}

	/// <summary>
	/// Try to insert a w×h rectangle. Returns its placement or null.
	/// </summary>
	public SFRectI? Insert(int w, int h)
	{
		int bestIndex = -1, bestX = 0, bestY = int.MaxValue;

		// find the skyline node giving the lowest y (and then leftmost)
		for (int i = 0; i < _nodes.Count; i++)
		{
			int y = Fit(i, w, h);
			if (y >= 0 && (y < bestY || (y == bestY && _nodes[i].X < bestX)))
			{
				bestIndex = i;
				bestX = _nodes[i].X;
				bestY = y;
			}
		}

		if (bestIndex == -1) return null;

		var rect = new SFRectI(bestX, bestY, w, h);
		AddSkylineLevel(bestIndex, rect);
		Merge();
		return rect;
	}

	// Check if rect fits at node index; return the y position or -1
	private int Fit(int index, int w, int h)
	{
		var node = _nodes[index];
		if (node.X + w > _pageWidth) return -1;

		int x = node.X;
		int width = w;
		int y = node.Y;
		int i = index;

		while (width > 0)
		{
			if (i >= _nodes.Count) return -1;
			y = Math.Max(y, _nodes[i].Y);
			if (y + h > _pageHeight) return -1;
			width -= _nodes[i].Width;
			i++;
		}
		return y;
	}

	// Carve out rect at node index and insert a new node
	private void AddSkylineLevel(int index, SFRectI rect)
	{
		var node = _nodes[index];
		var newNode = new Node(rect.Left, rect.Top + rect.Height, rect.Width);
		_nodes.Insert(index, newNode);

		// shrink or remove overlapping nodes
		for (int i = index + 1; i < _nodes.Count; i++)
		{
			var n = _nodes[i];
			if (n.X < newNode.X + newNode.Width)
			{
				int shrink = (newNode.X + newNode.Width) - n.X;
				n.X += shrink;
				n.Width -= shrink;
				if (n.Width <= 0)
				{
					_nodes.RemoveAt(i);
					i--;
				}
			}
			else break;
		}
	}

	// Merge adjacent nodes at the same height
	private void Merge()
	{
		for (int i = 0; i < _nodes.Count - 1; i++)
		{
			var a = _nodes[i];
			var b = _nodes[i + 1];
			if (a.Y == b.Y)
			{
				a.Width += b.Width;
				_nodes.RemoveAt(i + 1);
				i--;
			}
		}
	}
}
