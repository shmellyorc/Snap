namespace System;

public static class StringExtentions
{
	public static bool IsEmpty(this string v) =>
	
		string.IsNullOrWhiteSpace(v);
	public static bool IsNotEmpty(this string v) =>
		!IsEmpty(v);

	public static string ToEnumString(this Enum v) =>
		$"{v.GetType()}.{v}";

	public static bool IsNumeric(this string v)
	{
		return long.TryParse(v, out _);
	}

	public static string TrimToLength(this string v, int maxLength) =>
		v.Length > maxLength ? v.Substring(0, maxLength) : v;

}
