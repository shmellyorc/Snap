namespace System;

public static class RandExtentions
{
	public static T Choice<T>(this FastRandom rng, params T[] items)
	{
		if (rng == null) throw new ArgumentException(nameof(rng));
		if (items == null) throw new ArgumentNullException(nameof(items));
		if (items.Length == 0)
			throw new ArgumentException("Must provide at least one item.", nameof(items));

		int idx = rng.NextInt(items.Length);
		return items[idx];
	}

	public static T Choice<T>(this FastRandom rng, IReadOnlyList<T> list)
	{
		if (rng == null) throw new ArgumentException(nameof(rng));
		if (list == null) throw new ArgumentNullException(nameof(list));
		if (list.Count == 0)
			throw new ArgumentException("Must provide at least one item.", nameof(list));

		int idx = rng.NextInt(list.Count);
		return list[idx];
	}

	public static T Choice<T>(this FastRandom rng, IEnumerable<T> source)
	{
		if (rng == null) throw new ArgumentException(nameof(rng));
		if (source == null) throw new ArgumentNullException(nameof(source));

		if (source is List<T> list)
		{
			if (list.Count == 0)
				throw new ArgumentException("Sequence contains no elements.", nameof(source));
			int index = rng.NextInt(list.Count);
			return list[index];
		}

		T selected = default;
		int count = 0;
		foreach (var item in source)
		{
			count++;
			if (rng.NextInt(count) == 0)
				selected = item;
		}

		if (count == 0)
			throw new ArgumentException("Sequence contains no elements", nameof(source));
		return selected;
	}

	public static Vect2 RandomDirection(this FastRandom rng)
	{
		var dirs = new Vect2[]
		{
			Vect2.Up,
			Vect2.Right,
			Vect2.Down,
			Vect2.Left,
		};

		return Choice<Vect2>(rng, dirs);
	}

	public static Color RandomColor(this FastRandom rng) =>
		 new Color(rng.NextInt(0, 255), rng.NextInt(0, 255), rng.NextInt(0, 255));

	public static TEnum RandomEnum<TEnum>(this FastRandom rng) where TEnum : struct, Enum
	{
		var vals = Enum.GetValues<TEnum>();
		return vals[rng.NextInt(0, vals.Length)];
	}

	public static int NextSign(this FastRandom rng) => rng.NextBool() ? 1 : -1;

	public static void Shuffle<T>(this FastRandom rng, IList<T> list)
	{
		for (int i = list.Count - 1; i >= 0; i--)
		{
			int j = rng.RangeInt(0, i);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	public static int RollDice(this FastRandom rng, int diceCount, int sides)
	{
		int sum = 0;
		for (int i = diceCount - 1; i >= 0; i--)
			sum += rng.RangeInt(1, sides);
		return sum;
	}

	public static Vect2 RandomPointInCircle(this FastRandom rng)
	{
		float angle = rng.NextFloat(0f, MathF.PI * 2f);
		float radius = MathF.Sqrt(rng.NextFloat());
		return new Vect2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
	}

	public static Color RandomPastelColor(this FastRandom rng)
	{
		// bais twoards lighter values:
		byte r = (byte)rng.RangeInt(128, 255);
		byte g = (byte)rng.RangeInt(128, 255);
		byte b = (byte)rng.RangeInt(128, 255);
		return new Color(r, g, b);
	}
}
