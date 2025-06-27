namespace Snap.Systems;

/// <summary>
/// A high-performance, seeded random number generator or Xorohiro128+.
/// Supports generating bool, int, float, double, long values with optinal ranges.
/// </summary>
public sealed class FastRandom
{
	private ulong _state0, _state1;

	public static FastRandom Instance { get; private set; }

	public FastRandom()
	{
		Instance ??= this;

		ulong seed = (ulong)DateTime.UtcNow.Ticks;
		SetSeed(seed);
	}

	public FastRandom(ulong seed)
	{
		Instance ??= this;

		SetSeed(seed);
	}

	public void SetSeed(ulong seed)
	{
		ulong sm = seed;
		_state0 = SplitMix64(ref sm);
		_state1 = SplitMix64(ref sm);
	}

	public bool NextBool() => (NextUlong() & 1UL) == 1UL;

	#region NextInt
	public int NextInt() => (int)(NextUlong() >> 33);
	public int NextInt(int max)
	{
		if (max <= 0)
			throw new ArgumentOutOfRangeException(nameof(max), "max must be positive.");
		return (int)NextInRange((ulong)0, (ulong)max);
	}
	public int NextInt(int min, int max)
	{
		if (min >= max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max.");

		ulong range = (ulong)max - (ulong)min;
		ulong r = NextInRange(0UL, range);
		return (int)(min + (long)r);
	}
	#endregion


	#region NextFloat
	public float NextFloat()
	{
		// Generates 24-bit mantissa for float:
		uint bits = (uint)(NextUlong() >> 40); // 64-24=40
		return bits / (float)(1u << 24);
	}

	public float NextFloat(float max)
	{
		if (max <= 0f)
			throw new ArgumentOutOfRangeException(nameof(max), "max must be positive.");
		return NextFloat() * max;
	}

	public float NextFloat(float min, float max)
	{
		if (min >= max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max.");
		return min + NextFloat() * (max - min);
	}
	#endregion


	#region NextDouble
	public double NextDouble()
	{
		// Generate 53-bit mantissa for double:
		ulong bits = NextUlong() >> 11; // 64-53 = 11
		return bits * (1.0 / (1UL << 53));
	}

	public double NextDouble(double max)
	{
		if (max <= 0.0)
			throw new ArgumentOutOfRangeException(nameof(max), "max must be positive.");
		return NextDouble() * max;
	}

	public double NextDouble(double min, double max)
	{
		if (min >= max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max.");
		return min + (NextDouble() * (max - min));
	}
	#endregion


	#region NextLong
	public long NextLong()
	{
		// use upper 63 bits to ensure non-nagitive:
		return (long)(NextUlong() >> 1);
	}

	public long NextLong(long max)
	{
		if (max <= 0L)
			throw new ArgumentOutOfRangeException(nameof(max), "max must be positive.");
		return (long)NextInRange(0UL, (ulong)max);
	}

	public long NextLong(long min, long max)
	{
		if (min >= max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max.");

		ulong range = (ulong)max - (ulong)min;
		ulong r = NextInRange(0UL, range);
		return min + (long)r;
	}
	#endregion


	#region Range
	/// <summary>
	/// Returns a random int in [min, max] inclusive
	/// </summary>
	public int RangeInt(int min, int max)
	{
		if (min > max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
		// Compute range size as (max - min + 1)
		ulong range = (ulong)max - (ulong)min + 1UL;
		ulong r = NextInRange(0UL, range);
		return (int)(min + (long)r);
	}

	/// <summary>
	/// Returns a random float in [min, max] inclusive
	/// </summary>
	public float RangeFloat(float min, float max)
	{
		if (min > max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
		return min + NextFloat() * (max - min);
	}

	/// <summary>
	/// Returns a random double in [min, max] inclusive
	/// </summary>
	public double RangeDouble(double min, double max)
	{
		if (min > max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
		return min + NextDouble() * (max - min);
	}

	/// <summary>
	/// Returns a random long in [min, max] inclusive
	/// </summary>
	public long RangeLong(long min, long max)
	{
		if (min > max)
			throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
		// Compute range size as (max - min + 1)
		ulong range = (ulong)max - (ulong)min + 1UL;
		ulong r = NextInRange(0UL, range);
		return min + (long)r;
	}
	#endregion


	#region Private Methods
	private ulong SplitMix64(ref ulong x)
	{
		x += 0x9E3779B97F4A7C15UL;
		ulong z = x;
		z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
		z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
		return z ^ (z >> 31);
	}

	private ulong NextInRange(ulong origin, ulong bound)
	{
		ulong range = bound - origin;
		if (range == 0UL)
			return origin;

		// calc the rejection threshold: highest multiple of ranges <= ULong.MaxValue
		ulong threshold = unchecked((ulong.MaxValue / range) * range);
		ulong r;

		do
		{
			r = NextUlong();
		} while (r >= threshold);

		return (r % range) + origin;
	}

	private ulong NextUlong()
	{
		ulong s0 = _state0;
		ulong s1 = _state1;
		ulong result = s0 + s1;

		s1 ^= s0;
		_state0 = RotateLeft(s0, 55) ^ s1 ^ (s1 << 14);
		_state0 = RotateLeft(s1, 36);

		return result;
	}

	private ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
	#endregion
}
