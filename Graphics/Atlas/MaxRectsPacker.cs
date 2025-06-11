// using SFML.Graphics;

// public sealed class MaxRectsPacker
// {
// 	private readonly int _pageWidth;
// 	private readonly int _pageHeight;
// 	private readonly List<IntRect> _freeRectangles = new List<IntRect>();
// 	private readonly List<IntRect> _pendingRemovals = new List<IntRect>();


// 	public void Reset()
// 	{
// 		_freeRectangles.Clear();
// 		_freeRectangles.Add(new IntRect(0, 0, _pageWidth, _pageHeight));
// 		_pendingRemovals.Clear();
// 	}


// 	public MaxRectsPacker(int pageWidth, int pageHeight)
// 	{
// 		_pageWidth = pageWidth;
// 		_pageHeight = pageHeight;
// 		// Start with one big free rectangle covering the whole page
// 		_freeRectangles.Add(new IntRect(0, 0, pageWidth, pageHeight));
// 	}

// 	public IntRect? Insert(int w, int h)
// 	{
// 		FlushPendingRemovals();

// 		int bestShortSide = int.MaxValue;
// 		int bestIndex = -1;
// 		IntRect bestNode = default;

// 		for (int i = 0; i < _freeRectangles.Count; i++)
// 		{
// 			var free = _freeRectangles[i];
// 			if (free.Width >= w && free.Height >= h)
// 			{
// 				int leftoverHoriz = Math.Abs(free.Width - w);
// 				int leftoverVert = Math.Abs(free.Height - h);
// 				int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
// 				if (shortSideFit < bestShortSide)
// 				{
// 					bestShortSide = shortSideFit;
// 					bestNode = new IntRect(free.Left, free.Top, w, h);
// 					bestIndex = i;
// 				}
// 			}
// 		}
// 		if (bestIndex < 0)
// 			return null;

// 		SplitFreeNode(_freeRectangles[bestIndex], bestNode);
// 		_freeRectangles.RemoveAt(bestIndex);
// 		PruneFreeList();
// 		return bestNode;
// 	}

// 	public void RemoveLazy(IntRect rect)
// 	{
// 		_pendingRemovals.Add(rect);
// 	}

// 	internal void FlushPendingRemovals()
// 	{
// 		if (_pendingRemovals.Count == 0)
// 			return;

// 		foreach (var r in _pendingRemovals)
// 			_freeRectangles.Add(r);
// 		_pendingRemovals.Clear();

// 		MergeFreeList();
// 		PruneFreeList();
// 	}

// 	private void SplitFreeNode(IntRect freeRect, IntRect usedRect)
// 	{
// 		if (!RectanglesIntersect(freeRect, usedRect))
// 			return;

// 		// Top
// 		if (usedRect.Top > freeRect.Top && usedRect.Top < freeRect.Top + freeRect.Height)
// 		{
// 			_freeRectangles.Add(new IntRect(
// 				freeRect.Left,
// 				freeRect.Top,
// 				freeRect.Width,
// 				usedRect.Top - freeRect.Top));
// 		}
// 		// Bottom
// 		int freeBottom = freeRect.Top + freeRect.Height;
// 		int usedBottom = usedRect.Top + usedRect.Height;
// 		if (usedBottom < freeBottom)
// 		{
// 			_freeRectangles.Add(new IntRect(
// 				freeRect.Left,
// 				usedBottom,
// 				freeRect.Width,
// 				freeBottom - usedBottom));
// 		}
// 		// Left
// 		if (usedRect.Left > freeRect.Left && usedRect.Left < freeRect.Left + freeRect.Width)
// 		{
// 			_freeRectangles.Add(new IntRect(
// 				freeRect.Left,
// 				freeRect.Top,
// 				usedRect.Left - freeRect.Left,
// 				freeRect.Height));
// 		}
// 		// Right
// 		int freeRight = freeRect.Left + freeRect.Width;
// 		int usedRight = usedRect.Left + usedRect.Width;
// 		if (usedRight < freeRight)
// 		{
// 			_freeRectangles.Add(new IntRect(
// 				usedRight,
// 				freeRect.Top,
// 				freeRight - usedRight,
// 				freeRect.Height));
// 		}
// 	}

// 	private void PruneFreeList()
// 	{
// 		for (int i = 0; i < _freeRectangles.Count; i++)
// 		{
// 			var r1 = _freeRectangles[i];
// 			for (int j = i + 1; j < _freeRectangles.Count; j++)
// 			{
// 				var r2 = _freeRectangles[j];
// 				if (IsContainedIn(r1, r2))
// 				{
// 					_freeRectangles.RemoveAt(i);
// 					i--;
// 					break;
// 				}
// 				if (IsContainedIn(r2, r1))
// 				{
// 					_freeRectangles.RemoveAt(j);
// 					j--;
// 				}
// 			}
// 		}
// 	}

// 	private void MergeFreeList()
// 	{
// 		bool merged;
// 		do
// 		{
// 			merged = false;
// 			for (int i = 0; i < _freeRectangles.Count; i++)
// 			{
// 				var a = _freeRectangles[i];
// 				for (int j = i + 1; j < _freeRectangles.Count; j++)
// 				{
// 					var b = _freeRectangles[j];
// 					// Horizontal merge
// 					if (a.Top == b.Top && a.Height == b.Height)
// 					{
// 						if (a.Left + a.Width == b.Left)
// 						{
// 							var newRect = new IntRect(a.Left, a.Top, a.Width + b.Width, a.Height);
// 							_freeRectangles[i] = newRect;
// 							_freeRectangles.RemoveAt(j);
// 							merged = true;
// 							break;
// 						}
// 						if (b.Left + b.Width == a.Left)
// 						{
// 							var newRect = new IntRect(b.Left, b.Top, b.Width + a.Width, b.Height);
// 							_freeRectangles[i] = newRect;
// 							_freeRectangles.RemoveAt(j);
// 							merged = true;
// 							break;
// 						}
// 					}
// 					// Vertical merge
// 					if (a.Left == b.Left && a.Width == b.Width)
// 					{
// 						if (a.Top + a.Height == b.Top)
// 						{
// 							var newRect = new IntRect(a.Left, a.Top, a.Width, a.Height + b.Height);
// 							_freeRectangles[i] = newRect;
// 							_freeRectangles.RemoveAt(j);
// 							merged = true;
// 							break;
// 						}
// 						if (b.Top + b.Height == a.Top)
// 						{
// 							var newRect = new IntRect(b.Left, b.Top, b.Width, b.Height + a.Height);
// 							_freeRectangles[i] = newRect;
// 							_freeRectangles.RemoveAt(j);
// 							merged = true;
// 							break;
// 						}
// 					}
// 				}
// 				if (merged) break;
// 			}
// 		} while (merged);
// 	}

// 	private static bool IsContainedIn(IntRect a, IntRect b)
// 	{
// 		return a.Left >= b.Left && a.Top >= b.Top
// 			&& a.Left + a.Width <= b.Left + b.Width
// 			&& a.Top + a.Height <= b.Top + b.Height;
// 	}

// 	private static bool RectanglesIntersect(IntRect a, IntRect b)
// 	{
// 		return !(b.Left >= a.Left + a.Width
// 			  || b.Left + b.Width <= a.Left
// 			  || b.Top >= a.Top + a.Height
// 			  || b.Top + b.Height <= a.Top);
// 	}
// }
