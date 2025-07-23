namespace Snap.Systems;

public struct Color : IEquatable<Color>
{
	public static Color White => new(255, 255, 255);
	public static Color CornFlowerBlue => new(100, 149, 237);
	public static Color Black => new(0, 0, 0);
	public static Color Red => new(255, 0, 0);
	public static Color Green => new(0, 255, 0);
	public static Color Blue => new(0, 0, 255);
	public static Color Yellow => new(255, 255, 0);
	public static Color Cyan => new(0, 255, 255);
	public static Color Magenta => new(255, 0, 255);
	public static Color Transparent => new(0, 0, 0, 0);

	public byte R, G, B, A;

	public Color(byte r, byte g, byte b, byte a)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public Color(byte r, byte g, byte b) : this(r, g, b, 255) { }

	public Color(float r, float g, float b, float a)
		: this(
			(byte)Math.Clamp(r * 255f, 0f, 255f),
			(byte)Math.Clamp(g * 255f, 0f, 255f),
			(byte)Math.Clamp(b * 255f, 0f, 255f),
			(byte)Math.Clamp(a * 255f, 0f, 255f)
		)
	{ }

	public Color(float r, float g, float b)
		: this(r, g, b, 1.0f) { }

	public Color(string hex)
	{
		if (string.IsNullOrWhiteSpace(hex))
			throw new ArgumentNullException(nameof(hex));

		var r = hex.TrimStart('#');

		if (r.Length == 8)
		{
			A = (byte)int.Parse(r.Substring(0, 2), NumberStyles.HexNumber);
			R = (byte)int.Parse(r.Substring(2, 2), NumberStyles.HexNumber);
			G = (byte)int.Parse(r.Substring(4, 2), NumberStyles.HexNumber);
			B = (byte)int.Parse(r.Substring(6, 2), NumberStyles.HexNumber);
		}
		else if (r.Length == 6)
		{
			R = (byte)int.Parse(r.Substring(0, 2), NumberStyles.HexNumber);
			G = (byte)int.Parse(r.Substring(2, 2), NumberStyles.HexNumber);
			B = (byte)int.Parse(r.Substring(4, 2), NumberStyles.HexNumber);
			A = 255;
		}
		else if (r.Length == 3)
		{
			R = (byte)int.Parse(r.Substring(0, 1), NumberStyles.HexNumber);
			G = (byte)int.Parse(r.Substring(1, 1), NumberStyles.HexNumber);
			B = (byte)int.Parse(r.Substring(2, 1), NumberStyles.HexNumber);
			A = 255;
		}
		else
			throw new Exception();
	}


	#region Operator: ==, !=
	public static bool operator ==(in Color a, in Color b) => a.Equals(b);
	public static bool operator !=(in Color a, in Color b) => !a.Equals(b);
	#endregion


	#region Operator: +
	public static Color operator +(in Color a, in Color b) => new(
			(byte)Math.Clamp(a.R + b.R, 0, 255),
			(byte)Math.Clamp(a.G + b.G, 0, 255),
			(byte)Math.Clamp(a.B + b.B, 0, 255),
			(byte)Math.Clamp(a.A + b.A, 0, 255)
		);
	public static Color operator +(in Color a, float b) => new(
		(byte)Math.Clamp(a.R + b, 0f, 255f),
		(byte)Math.Clamp(a.G + b, 0f, 255f),
		(byte)Math.Clamp(a.B + b, 0f, 255f),
		(byte)Math.Clamp(a.A + b, 0f, 255f)
	);
	public static Color operator +(float a, in Color b) => new(
		(byte)Math.Clamp(b.R + a, 0f, 255f),
		(byte)Math.Clamp(b.G + a, 0f, 255f),
		(byte)Math.Clamp(b.B + a, 0f, 255f),
		(byte)Math.Clamp(b.A + a, 0f, 255f)
	);
	#endregion


	#region Operator: *
	public static Color operator *(in Color a, in Color b) => new(
			(byte)Math.Clamp(a.R * b.R, 0, 255),
			(byte)Math.Clamp(a.G * b.G, 0, 255),
			(byte)Math.Clamp(a.B * b.B, 0, 255),
			(byte)Math.Clamp(a.A * b.A, 0, 255)
		);
	public static Color operator *(Color a, float b) => new(
		(byte)Math.Clamp(a.R * b, 0f, 255f),
		(byte)Math.Clamp(a.G * b, 0f, 255f),
		(byte)Math.Clamp(a.B * b, 0f, 255f),
		(byte)Math.Clamp(a.A * b, 0f, 255f)
	);
	public static Color operator *(float a, in Color b) => new(
		(byte)Math.Clamp(b.R * a, 0f, 255f),
		(byte)Math.Clamp(b.G * a, 0f, 255f),
		(byte)Math.Clamp(b.B * a, 0f, 255f),
		(byte)Math.Clamp(b.A * a, 0f, 255f)
	);
	#endregion


	#region Operator: /
	public static Color operator /(in Color a, in Color b) => new(
			(byte)Math.Clamp(a.R / b.R, 0, 255),
			(byte)Math.Clamp(a.G / b.G, 0, 255),
			(byte)Math.Clamp(a.B / b.B, 0, 255),
			(byte)Math.Clamp(a.A / b.A, 0, 255)
		);
	public static Color operator /(in Color a, float b) => new(
		(byte)Math.Clamp(a.R / b, 0f, 255f),
		(byte)Math.Clamp(a.G / b, 0f, 255f),
		(byte)Math.Clamp(a.B / b, 0f, 255f),
		(byte)Math.Clamp(a.A / b, 0f, 255f)
	);
	public static Color operator /(float a, in Color b) => new(
		(byte)Math.Clamp(b.R / a, 0f, 255f),
		(byte)Math.Clamp(b.G / a, 0f, 255f),
		(byte)Math.Clamp(b.B / a, 0f, 255f),
		(byte)Math.Clamp(b.A / a, 0f, 255f)
	);
	#endregion


	#region Operator: -
	public static Color operator -(in Color a, in Color b) => new(
			(byte)Math.Clamp(a.R - b.R, 0, 255),
			(byte)Math.Clamp(a.G - b.G, 0, 255),
			(byte)Math.Clamp(a.B - b.B, 0, 255),
			(byte)Math.Clamp(a.A - b.A, 0, 255)
		);
	public static Color operator -(in Color a, float b) => new(
		(byte)Math.Clamp(a.R - b, 0f, 255f),
		(byte)Math.Clamp(a.G - b, 0f, 255f),
		(byte)Math.Clamp(a.B - b, 0f, 255f),
		(byte)Math.Clamp(a.A - b, 0f, 255f)
	);
	public static Color operator -(float a, in Color b) => new(
		(byte)Math.Clamp(b.R - a, 0f, 255f),
		(byte)Math.Clamp(b.G - a, 0f, 255f),
		(byte)Math.Clamp(b.B - a, 0f, 255f),
		(byte)Math.Clamp(b.A - a, 0f, 255f)
	);
	#endregion


	#region implicit Operators
	public static implicit operator SFColor(in Color a) => new(a.R, a.G, a.B, a.A);
	#endregion


	#region IEquatable
	public readonly bool Equals(Color other) =>
			R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

	public readonly override bool Equals([NotNullWhen(true)] object obj) =>
		obj is Color value && Equals(value);

	public readonly override int GetHashCode() => HashCode.Combine(R, G, B, A);

	public readonly override string ToString() => $"Color({R},{G},{B},{A})";
	#endregion
}
