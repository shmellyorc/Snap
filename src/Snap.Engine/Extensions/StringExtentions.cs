namespace System;

/// <summary>
/// Provides extension methods for common <see cref="string"/> operations.
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// Determines whether the string is <c>null</c>, empty, or consists only of white-space characters.
	/// </summary>
	/// <param name="v">The string to test.</param>
	/// <returns><c>true</c> if <paramref name="v"/> is <c>null</c>, empty, or whitespace; otherwise, <c>false</c>.</returns>
	public static bool IsEmpty(this string v) =>
		string.IsNullOrWhiteSpace(v);

	/// <summary>
	/// Determines whether the string is not <c>null</c>, empty, or whitespace.
	/// </summary>
	/// <param name="v">The string to test.</param>
	/// <returns><c>true</c> if <paramref name="v"/> contains any non-whitespace characters; otherwise, <c>false</c>.</returns>
	public static bool IsNotEmpty(this string v) =>
		!IsEmpty(v);

	/// <summary>
	/// Converts an <see cref="Enum"/> value to its fully qualified string representation.
	/// </summary>
	/// <param name="v">The enum value.</param>
	/// <returns>A string in the format "Namespace.TypeName.Value".</returns>
	public static string ToEnumString(this Enum v) =>
		$"{v.GetType().FullName}.{v}";

	/// <summary>
	/// Determines whether the string represents a valid integer number.
	/// </summary>
	/// <param name="v">The string to test.</param>
	/// <returns><c>true</c> if <paramref name="v"/> can be parsed to a <see cref="long"/>; otherwise, <c>false</c>.</returns>
	public static bool IsNumeric(this string v) =>
		long.TryParse(v, out _);

	/// <summary>
	/// Truncates the string to the specified maximum length, if necessary.
	/// </summary>
	/// <param name="v">The string to trim.</param>
	/// <param name="maxLength">The maximum allowed length of the returned string.</param>
	/// <returns>
	/// The original string if its length is less than or equal to <paramref name="maxLength"/>,
	/// otherwise a substring of <paramref name="v"/> of length <paramref name="maxLength"/>.
	/// </returns>
	public static string TrimToLength(this string v, int maxLength) =>
		v.Length > maxLength ? v.Substring(0, maxLength) : v;

	/// <summary>
	/// Counts how many times a specific character appears in the string.
	/// </summary>
	/// <param name="input">The input string to search.</param>
	/// <param name="target">The character to count.</param>
	/// <returns>The number of times the character appears in the string.</returns>
	public static int CountChar(this string input, char target)
	{
		if (string.IsNullOrEmpty(input))
			return 0;

		int count = 0;
		foreach (char c in input)
		{
			if (c == target)
				count++;
		}

		return count;
	}
}

