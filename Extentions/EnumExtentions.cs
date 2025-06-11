// using System.Runtime.InteropServices;

// namespace System;

// public static class EnumExtensions
// {
// 	public static T AddFlag<T>(this T value, T flag) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);
// 		dynamic f = Convert.ChangeType(flag, underlyingType);

// 		return (T)Enum.ToObject(typeof(T), v | f);
// 	}

// 	public static T AddFlags<T>(this T value, params T[] flags) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);

// 		foreach (var flag in flags)
// 		{
// 			dynamic f = Convert.ChangeType(flag, underlyingType);
// 			v |= f;
// 		}

// 		return (T)Enum.ToObject(typeof(T), v);
// 	}


// 	public static T RemoveFlag<T>(this T value, T flag) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);
// 		dynamic f = Convert.ChangeType(flag, underlyingType);

// 		return (T)Enum.ToObject(typeof(T), v & ~f);
// 	}

// 	public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);
// 		dynamic f = Convert.ChangeType(flag, underlyingType);

// 		return (v & f) != 0;
// 	}

// 	public static T ToggleFlag<T>(this T value, T flag) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);
// 		dynamic f = Convert.ChangeType(flag, underlyingType);

// 		return (T)Enum.ToObject(typeof(T), v ^ f);
// 	}

// 	public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum
// 	{
// 		foreach (T flag in Enum.GetValues(typeof(T)))
// 		{
// 			if (value.HasFlag(flag))
// 				yield return flag;
// 		}
// 	}

// 	public static T CombineFlags<T>(this T value, params T[] flags) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);

// 		foreach (var flag in flags)
// 		{
// 			dynamic f = Convert.ChangeType(flag, underlyingType);
// 			v |= f;
// 		}

// 		return (T)Enum.ToObject(typeof(T), v);
// 	}

// 	public static T ClearAllFlags<T>(this T value) where T : struct, Enum
// 	{
// 		return default; // Explicitly returns the default zero value for the enum type
// 	}

// 	public static string ToBinaryString<T>(this T value) where T : struct, Enum
// 	{
// 		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
// 		dynamic v = Convert.ChangeType(value, underlyingType);

// 		int bitSize = Marshal.SizeOf(v) * 8; // Ensure correct bit-length padding
// 		return Convert.ToString((int)v, 2).PadLeft(bitSize, '0');
// 	}

// 	public static List<T> GetActiveFlags<T>(this T value) where T : struct, Enum =>
// 	Enum.GetValues(typeof(T)).Cast<T>().Where(flag => value.HasFlag(flag)).ToList();

// 	public static int CountFlags<T>(this T value) where T : struct, Enum =>
// 		Enum.GetValues(typeof(T)).Cast<T>().Count(flag => value.HasFlag(flag));
// }