namespace Snap.Engine.Systems;

public struct Rect2 : IEquatable<Rect2>
{
	private const float Epsilon = 1e-6f;

	#region Properties
	public float X, Y, Width, Height;

	public readonly float Left => X;
	public readonly float Top => Y;
	public readonly float Right => X + Width;
	public readonly float Bottom => Y + Height;

	public static Rect2 Zero => new(0, 0, 0, 0);

	public Vect2 Center
	{
		readonly get => new(X + Width * 0.5f, Y + Height * 0.5f);
		set
		{
			// Setting a new center must move X,Y so that the rect is centered there.
			X = value.X - Width * 0.5f;
			Y = value.Y - Height * 0.5f;
		}
	}

	public readonly bool IsZero => MathF.Abs(Width) <= Epsilon && MathF.Abs(Height) <= Epsilon;
	public readonly float Area => Width * Height;
	public readonly float AspectRatio => Height != 0f ? Width / Height : 0f;
	public readonly Vect2 TopLeft => new(Left, Top);
	public readonly Vect2 TopRight => new(Right, Top);
	public readonly Vect2 BottomLeft => new(Left, Bottom);
	public readonly Vect2 BottomRight => new(Right, Bottom);
	public readonly Vect2 MidTop => new(Center.X, Top);
	public readonly Vect2 MidBottom => new(Center.X, Bottom);
	public readonly Vect2 MidLeft => new(Left, Center.Y);
	public readonly Vect2 MidRight => new(Right, Center.Y);

	public Vect2 Position
	{
		get => new(X, Y);
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public Vect2 Size
	{
		get => new(Width, Height);
		set
		{
			Width = value.X;
			Height = value.Y;
		}
	}
	#endregion


	#region Constuctors
	public Rect2(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public Rect2(in Vect2 position, in Vect2 size)
		: this(position.X, position.Y, size.X, size.Y) { }

	internal Rect2(SFRectF rect) : this(rect.Left, rect.Top, rect.Width, rect.Height) { }

	internal Rect2(SFRectI rect) : this(rect.Left, rect.Top, rect.Width, rect.Height) { }

	public readonly void Deconstruct(out float x, out float y, out float w, out float h)
	{
		x = X;
		y = Y;
		w = Width;
		h = Height;
	}
	#endregion


	#region Operator: ==, !=
	public static bool operator ==(in Rect2 a, in Rect2 b) => a.Equals(b);
	public static bool operator !=(in Rect2 a, in Rect2 b) => !a.Equals(b);
	#endregion


	#region Operator: + / -
	public static Rect2 operator +(in Rect2 a, float b)
	{
		return new Rect2(
			a.X - b,
			a.Y - b,
			a.Width + b * 2f,
			a.Height + b * 2f
		);
	}
	public static Rect2 operator +(in Rect2 a, in Vect2 b)
		=> new(a.X + b.X, a.Y + b.Y, a.Width, a.Height);

	public static Rect2 operator -(in Rect2 a, float b) => a + -b;
	public static Rect2 operator -(in Rect2 a, in Vect2 b)
		=> new(a.X - b.X, a.Y - b.Y, a.Width, a.Height);
	#endregion


	#region Implicit Operators
	public static implicit operator SFRectF(in Rect2 v) =>
		new(v.X, v.Y, v.Width, v.Height);
	public static implicit operator SFRectI(in Rect2 v) =>
		new((int)v.X, (int)v.Y, (int)v.Width, (int)v.Height);
	#endregion


	#region IEquatable
	public readonly bool Equals(Rect2 other) =>
		X.Equals(other.X) && Y.Equals(other.Y) &&
		Width.Equals(other.Width) && Height.Equals(other.Height);

	public readonly override bool Equals([NotNullWhen(true)] object obj) =>
		obj is Rect2 value && Equals(value);

	public readonly override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

	public readonly override string ToString() => $"Rect({X},{Y},{Width},{Height})";
	#endregion


	#region Contains
	public bool Contains(float px, float py)
	{
		return px >= Left && px < Right
			&& py >= Top && py < Bottom;
	}

	public bool Contains(in Vect2 point)
		=> Contains(point.X, point.Y);

	public bool Contains(in Rect2 other)
	{
		return other.Left >= Left
			&& other.Right <= Right
			&& other.Top >= Top
			&& other.Bottom <= Bottom;
	}
	#endregion


	#region Intersects
	public bool Intersects(in Rect2 other)
	{
		return other.Left < Right
			&& Left < other.Right
			&& other.Top < Bottom
			&& Top < other.Bottom;
	}

	public static Rect2 Intersection(in Rect2 a, in Rect2 b)
	{
		float x1 = MathF.Max(a.Left, b.Left);
		float y1 = MathF.Max(a.Top, b.Top);
		float x2 = MathF.Min(a.Right, b.Right);
		float y2 = MathF.Min(a.Bottom, b.Bottom);

		if (x2 <= x1 || y2 <= y1)
		{
			// No overlap
			return Zero;
		}

		return new Rect2(x1, y1, x2 - x1, y2 - y1);
	}

	public readonly Rect2 Intersection(in Rect2 other)
		=> Intersection(this, other);
	#endregion


	#region Union
	public readonly Rect2 Union(in Rect2 other)
		=> Union(this, other);
	public static Rect2 Union(in Rect2 a, in Rect2 b)
	{
		float x1 = MathF.Min(a.Left, b.Left);
		float y1 = MathF.Min(a.Top, b.Top);
		float x2 = MathF.Max(a.Right, b.Right);
		float y2 = MathF.Max(a.Bottom, b.Bottom);

		return new Rect2(x1, y1, x2 - x1, y2 - y1);
	}
	#endregion


	#region Inflate
	public readonly Rect2 Pad(float amount) => Inflate(this, amount, amount);
	public readonly Rect2 Inflate(float dx, float dy) => Inflate(this, dx, dy);
	public static Rect2 Inflate(in Rect2 r, float dx, float dy)
		=> new Rect2(
			r.X - dx,
			r.Y - dy,
			r.Width + dx * 2f,
			r.Height + dy * 2f
		);
	#endregion


	#region Offset
	public readonly Rect2 Offset(float dx, float dy) => Offset(this, dx, dy);
	public static Rect2 Offset(in Rect2 r, float dx, float dy)
		=> new(r.X + dx, r.Y + dy, r.Width, r.Height);

	public readonly Rect2 Offset(in Vect2 delta) => Offset(this, delta);
	public static Rect2 Offset(in Rect2 r, in Vect2 delta)
		=> new(r.X + delta.X, r.Y + delta.Y, r.Width, r.Height);
	#endregion


	#region Compare
	public int CompareTo(Rect2 other)
	{
		int cmp = X.CompareTo(other.X);
		if (cmp != 0) return cmp;
		return Y.CompareTo(other.Y);
	}

	public static int CompareByArea(in Rect2 a, in Rect2 b)
		=> a.Area.CompareTo(b.Area);
	#endregion


	#region Enclose
	public void Enclose(in Vect2 point)
	{
		float minX = MathF.Min(Left, point.X);
		float minY = MathF.Min(Top, point.Y);
		float maxX = MathF.Max(Right, point.X);
		float maxY = MathF.Max(Bottom, point.Y);

		X = minX;
		Y = minY;
		Width = maxX - minX;
		Height = maxY - minY;
	}

	public static Rect2 Enclose(in Rect2 r, in Vect2 point)
	{
		float minX = MathF.Min(r.Left, point.X);
		float minY = MathF.Min(r.Top, point.Y);
		float maxX = MathF.Max(r.Right, point.X);
		float maxY = MathF.Max(r.Bottom, point.Y);

		return new Rect2(minX, minY, maxX - minX, maxY - minY);
	}
	#endregion


	#region FromCenter
	public static Rect2 FromCenter(in Vect2 center, in Vect2 size)
	{
		float halfW = size.X * 0.5f;
		float halfH = size.Y * 0.5f;
		return new Rect2(center.X - halfW,
						 center.Y - halfH,
						 size.X,
						 size.Y);
	}

	public static Rect2 FromCenter(float centerX, float centerY, float width, float height)
	{
		float halfW = width * 0.5f;
		float halfH = height * 0.5f;
		return new Rect2(centerX - halfW,
						 centerY - halfH,
						 width,
						 height);
	}
	#endregion


	#region Clamp
	public readonly Vect2 ClampPoint(in Vect2 point)
	{
		float cx = MathF.Min(MathF.Max(point.X, Left), Right);
		float cy = MathF.Min(MathF.Max(point.Y, Top), Bottom);

		return new Vect2(cx, cy);
	}

	public readonly Rect2 CropTo(in Rect2 bounds)
		=> Intersection(this, bounds);
	#endregion
}
