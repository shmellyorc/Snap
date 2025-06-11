namespace System;

public static class IEnumerableExtentions
{
	public static bool IsEmpty<T>(this IEnumerable<T> v) =>
		v == null || !v.Any();
	public static bool IsNotEmpty<T>(this IEnumerable<T> v) =>
		!IsEmpty<T>(v);

	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach (var item in source)
			action(item);
	}

	public static T? SafeElementAt<T>(this IEnumerable<T> source, int index) =>
		index >= 0 && index < source.Count() ? source.ElementAt(index) : default;

	public static int IndexOf<T>(this IEnumerable<T> source, T item) =>
		source.Select((value, index) => (value, index))
			  .FirstOrDefault(x => EqualityComparer<T>.Default.Equals(x.value, item)).index;

	public static (IEnumerable<T> matches, IEnumerable<T> nonMatches) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		var matches = source.Where(predicate);
		var nonMatches = source.Where(x => !predicate(x));
		
		return (matches, nonMatches);
	}

}
