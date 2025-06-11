using Snap.Helpers;

namespace Snap.Systems;

public struct Vect2 : IEquatable<Vect2>
{
	private const float Epsilon = 1e-6f;

	public float X, Y;

	#region Properties
	public static Vect2 Zero => new(0, 0);
	public static Vect2 One => new(1, 1);
	public static Vect2 Up => new(0, -1);
	public static Vect2 Right => new(1, 0);
	public static Vect2 Down => new(0, 1);
	public static Vect2 Left => new(-1, 0);

	public readonly bool IsZero => MathF.Abs(X) < Epsilon && MathF.Abs(Y) < Epsilon;
	#endregion

	#region Constructors
	public Vect2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Vect2(float value) : this(value, value) { }

	// Deep Clone:
	public Vect2(Vect2 value) : this(value.X, value.Y) { }

	public readonly void Deconstruct(out float x, out float y)
	{
		x = X;
		y = Y;
	}
	#endregion


	#region Operator: ==, !=
	public static bool operator ==(in Vect2 a, in Vect2 b) => a.Equals(b);
	public static bool operator !=(in Vect2 a, in Vect2 b) => !a.Equals(b);
	#endregion


	#region Operator: +
	public static Vect2 operator +(in Vect2 a, in Vect2 b) => new(a.X + b.X, a.Y + b.Y);
	public static Vect2 operator +(in Vect2 a, float b) => new(a.X + b, a.Y + b);
	public static Vect2 operator +(float a, in Vect2 b) => new(a + b.X, a + b.Y);
	#endregion


	#region Operator: *
	public static Vect2 operator *(in Vect2 a, in Vect2 b)
	{
		// Improves precision by avoiding extra rounding steps
		float x = MathF.FusedMultiplyAdd(a.X, b.X, 0f);
		float y = MathF.FusedMultiplyAdd(a.Y, b.Y, 0f);
		return new Vect2(x, y);
	}

	public static Vect2 operator *(in Vect2 a, float b)
	{
		float x = MathF.FusedMultiplyAdd(a.X, b, 0f);
		float y = MathF.FusedMultiplyAdd(a.Y, b, 0f);
		return new Vect2(x, y);
	}

	public static Vect2 operator *(float a, in Vect2 b) => b * a;
	#endregion


	#region Operator: /
	public static Vect2 operator /(in Vect2 a, in Vect2 b)
	{
		float x = MathF.Abs(b.X) > Epsilon ? a.X / b.X : 0f;
		float y = MathF.Abs(b.Y) > Epsilon ? a.Y / b.Y : 0f;
		return new Vect2(x, y);
	}

	public static Vect2 operator /(in Vect2 a, float b)
	{
		// Avoid division by values close to zero
		float safeB = MathF.Abs(b) > Epsilon ? b : 1f;
		return new Vect2(a.X / safeB, a.Y / safeB);
	}

	public static Vect2 operator /(float a, in Vect2 b)
	{
		float x = MathF.Abs(b.X) > Epsilon ? a / b.X : 0f;
		float y = MathF.Abs(b.Y) > Epsilon ? a / b.Y : 0f;
		return new Vect2(x, y);
	}
	#endregion


	#region Operator: -
	public static Vect2 operator -(in Vect2 value) => new(-value.X, -value.Y);
	public static Vect2 operator -(in Vect2 a, in Vect2 b) => new(a.X - b.X, a.Y - b.Y);
	public static Vect2 operator -(in Vect2 a, float b) => new(a.X - b, a.Y - b);
	public static Vect2 operator -(float a, in Vect2 b) => new(a - b.X, a - b.Y);
	#endregion


	#region Operator: implicit
	public static implicit operator SFVectF(in Vect2 v) => new(v.X, v.Y);
	public static implicit operator SFVectI(in Vect2 v) => new((int)MathF.Round(v.X), (int)MathF.Round(v.Y));
	public static implicit operator SFVectU(in Vect2 v) => new((uint)MathF.Round(v.X), (uint)MathF.Round(v.Y));
	public static implicit operator Vect2(in SFVectF v) => new(v.X, v.Y);
	public static implicit operator Vect2(in SFVectI v) => new(v.X, v.Y);
	public static implicit operator Vect2(in SFVectU v) => new(v.X, v.Y);

	#endregion


	// public static explicit operator Vect2(in SFVectI v) => new(v.X, v.Y);


	#region IEquatable
	public readonly bool Equals(Vect2 other) =>
		MathF.Abs(X - other.X) < Epsilon && MathF.Abs(Y - other.Y) < Epsilon;

	public readonly override bool Equals([NotNullWhen(true)] object obj) =>
		obj is Vect2 value && Equals(value);

	public readonly override int GetHashCode() => HashCode.Combine(X, Y);

	public readonly override string ToString() => $"Vect({X}, {Y})";
	#endregion


	#region Length
	public readonly float LengthSquared() => LengthSquared(this);
	public static float LengthSquared(in Vect2 value)
		=> value.X * value.X + value.Y * value.Y;
	public readonly float Length() => Length(this);
	public static float Length(in Vect2 value)
		=> MathF.Sqrt(LengthSquared(value));
	#endregion


	#region NearlyEquals
	public readonly bool NearlyEquals(in Vect2 other, float epsilon = Epsilon) =>
		NearlyEquals(this, other, epsilon);
	public static bool NearlyEquals(in Vect2 a, in Vect2 b, float epsilon = Epsilon) =>
		MathF.Abs(a.X - b.X) <= epsilon && MathF.Abs(a.Y - b.Y) <= epsilon;
	#endregion


	#region Sign
	public readonly Vect2 Sign() => Sign(this);
	public static Vect2 Sign(Vect2 value) =>
		new(MathF.Sign(value.X), MathF.Sign(value.Y));
	#endregion


	#region Normalize
	public readonly Vect2 Normalize() => Normalize(this);
	public static Vect2 Normalize(in Vect2 value)
	{
		var len = Length(value);
		return (len > 0f)
			? new Vect2(value.X / len, value.Y / len)
			: new Vect2(0f, 0f);
	}
	#endregion


	#region RotateAround
	public readonly Vect2 RotateAround(in Vect2 pivot, float radians) => RotateAround(this, pivot, radians);
	public static Vect2 RotateAround(in Vect2 point, in Vect2 pivot, float radians)
	{
		// Translate so pivot is origin
		float dx = point.X - pivot.X;
		float dy = point.Y - pivot.Y;
		float cos = MathF.Cos(radians);
		float sin = MathF.Sin(radians);
		float rx = dx * cos - dy * sin;
		float ry = dx * sin + dy * cos;
		// Translate back
		return new Vect2(rx + pivot.X, ry + pivot.Y);
	}
	#endregion


	#region Min
	public readonly Vect2 Min(in Vect2 other) => Min(this, other);
	public static Vect2 Min(in Vect2 a, in Vect2 b)
		=> new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));
	#endregion


	#region Max
	public readonly Vect2 Max(in Vect2 other) => Max(this, other);
	public static Vect2 Max(in Vect2 a, in Vect2 b)
		=> new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));
	#endregion


	#region Abs
	public readonly Vect2 Abs() => Abs(this);
	public static Vect2 Abs(in Vect2 a)
		=> new(MathF.Abs(a.X), MathF.Abs(a.Y));
	#endregion


	#region Floor
	public readonly Vect2 Floor() => Floor(this);
	public static Vect2 Floor(in Vect2 a)
		=> new(MathF.Floor(a.X), MathF.Floor(a.Y));
	#endregion


	#region Ceiling
	public readonly Vect2 Ceiling() => Ceiling(this);
	public static Vect2 Ceiling(in Vect2 a)
		=> new(MathF.Ceiling(a.X), MathF.Ceiling(a.Y));
	#endregion


	#region Clamp
	public readonly Vect2 Clamp(in Vect2 min, in Vect2 max) => Clamp(this, min, max);
	public static Vect2 Clamp(in Vect2 value, in Vect2 min, in Vect2 max) =>
		new(
			MathF.Min(MathF.Max(value.X, min.X), max.X),
			MathF.Min(MathF.Max(value.Y, min.Y), max.Y)
		);
	#endregion


	#region Center
	public readonly Vect2 Center(in Vect2 other, bool clamped) => Center(this, other, clamped);
	public static Vect2 Center(in Vect2 value, in Vect2 other, bool clamped) => new(
		MathHelpers.Center(value.X, other.X, clamped),
		MathHelpers.Center(value.Y, other.Y, clamped)
	);
	#endregion


	#region Distance
	public readonly float DistanceSquared(in Vect2 other) => DistanceSquared(this, other);
	public static float DistanceSquared(in Vect2 a, in Vect2 b)
		=> (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
	public readonly float Distance(in Vect2 other) => Distance(this, other);
	public static float Distance(in Vect2 a, in Vect2 b)
		=> (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

	/// <summary>Manhattan (“L1”) distance between a and b.</summary>
	public readonly float ManhattanDistance(in Vect2 other) => ManhattanDistance(this, other);
	/// <summary>Manhattan (“L1”) distance between a and b.</summary>
	public static float ManhattanDistance(in Vect2 a, in Vect2 b)
		=> MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);
	#endregion


	#region Dot
	public readonly float Dot(in Vect2 other) => Dot(this, other);
	public static float Dot(in Vect2 a, in Vect2 b) => a.X * b.X + a.Y * b.Y;
	#endregion


	#region Cross
	public readonly float Cross(in Vect2 other) => Cross(this, other);
	public static float Cross(in Vect2 a, in Vect2 b) => a.X * b.Y - a.Y * b.X;
	#endregion


	#region Add
	public readonly Vect2 Add(in Vect2 other) => Add(this, other);
	public static Vect2 Add(in Vect2 a, in Vect2 b) => new(a.X + b.X, a.Y + b.Y);
	#endregion


	#region Subtract
	public readonly Vect2 Subtract(in Vect2 other) => Subtract(this, other);
	public static Vect2 Subtract(in Vect2 a, in Vect2 b) => new(a.X - b.X, a.Y - b.Y);
	#endregion


	#region Multiply
	public readonly Vect2 Multiply(float amount) => Multiply(this, amount);
	public static Vect2 Multiply(in Vect2 vector, float s) => new(vector.X * s, vector.Y * s);
	#endregion


	#region Divide
	public readonly Vect2 Divide(float amount) => Divide(this, amount);
	public static Vect2 Divide(in Vect2 vector, float amount) => new(vector.X / amount, vector.Y / amount);
	#endregion


	#region MoveTowards
	public readonly Vect2 MoveTowards(in Vect2 target, float maxDelta)
		=> MoveTowards(this, target, maxDelta);
	public static Vect2 MoveTowards(in Vect2 current, in Vect2 target, float maxDelta)
	{
		// Vector from current → target
		float dx = target.X - current.X;
		float dy = target.Y - current.Y;
		float distSq = dx * dx + dy * dy;
		if (distSq == 0f || maxDelta <= 0f)
			return current;

		float dist = MathF.Sqrt(distSq);
		if (dist <= maxDelta)
		{
			// Already within maxDelta; snap to target
			return target;
		}

		// Move along the direction by maxDelta
		float scale = maxDelta / dist;
		return new Vect2(
			current.X + dx * scale,
			current.Y + dy * scale
		);
	}
	#endregion


	#region Lerp
	public readonly Vect2 Lerp(in Vect2 other, float t) => Lerp(this, other, t);
	public static Vect2 Lerp(in Vect2 a, in Vect2 b, float t)
		=> new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

	public readonly Vect2 SmoothStep(in Vect2 target, float t)
		=> SmoothStep(this, target, t);
	public static Vect2 SmoothStep(in Vect2 a, in Vect2 b, float t)
	{
		// clamp t to [0,1]
		t = Math.Clamp(t, 0f, 1f);

		// smooth‐step Hermite: 3t² – 2t³  (ease‐in/ease‐out)
		float tt = t * t * (3f - 2f * t);

		return new Vect2(
			a.X + (b.X - a.X) * tt,
			a.Y + (b.Y - a.Y) * tt
		);
	}
	#endregion


	#region Reflect
	public static Vect2 Reflect(in Vect2 v, in Vect2 normal)
	{
		var dot = v.Dot(normal);
		return new Vect2(
			v.X - 2f * dot * normal.X,
			v.Y - 2f * dot * normal.Y
		);
	}
	#endregion


	#region Angle & Rotation
	public readonly float Angle() => Angle(this);
	public static float Angle(Vect2 value) => MathF.Atan2(value.Y, value.X);

	public static Vect2 FromAngle(float radians) => new(MathF.Cos(radians), MathF.Sin(radians));

	public void Rotate(float radians)
	{
		var cos = MathF.Cos(radians);
		var sin = MathF.Sin(radians);
		var newX = X * cos - Y * sin;
		var newY = X * sin + Y * cos;
		X = newX;
		Y = newY;
	}

	public Vect2 Rotated(float radians)
	{
		var cos = MathF.Cos(radians);
		var sin = MathF.Sin(radians);
		return new Vect2(X * cos - Y * sin, X * sin + Y * cos);
	}
	#endregion
}
