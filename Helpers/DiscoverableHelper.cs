namespace Snap.Helpers;

public static class DiscoverableHelper
{
    private static readonly WeakReference<List<Type>> _allTypesRef = new(null);
    private static readonly object _allTypesLock = new();
    private static readonly ConcurrentDictionary<Type, DiscoverableAttribute?> _metaCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> _findAllCache = [];

    static DiscoverableHelper()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
        {
            _allTypesRef.SetTarget(null);
            _metaCache.Clear();
            _findAllCache.Clear();
        };
    }

    private static List<Type> AllTypes
    {
        get
        {
            if (!_allTypesRef.TryGetTarget(out var types) || types == null)
            {
                lock (_allTypesLock)
                {
                    if (!_allTypesRef.TryGetTarget(out types) || types == null)
                    {
                        types = AppDomain.CurrentDomain.GetAssemblies()
                            .Where(a => !a.IsDynamic)
                            .SelectMany(a =>
                            {
                                try { return a.GetTypes(); }
                                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
                            })
                            .Where(t => t != null && t.IsClass)
                            .Select(t => t!)
                            .ToList();

                        _allTypesRef.SetTarget(types);
                    }
                }
            }
            return types!;
        }
    }

    private static IEnumerable<(Type Type, DiscoverableAttribute Meta)> AllWithMeta() =>
        AllTypes.Select(t => (Type: t, Meta: _metaCache.GetOrAdd(t, GetMeta)))
                .Where(x => x.Meta != null)
                .Select(x => (x.Type, x.Meta!));

    private static IEnumerable<(Type Type, DiscoverableAttribute Meta)> Filter<T>() =>
        AllWithMeta()
            .Where(x =>
                typeof(T).IsAssignableFrom(x.Type)
                && x.Type != typeof(T)
                && x.Meta.Enabled
            );

    /// <summary>
    /// Retrieves all discoverable types implementing or inheriting T.
    /// Uses a cache per T to avoid repeated enumeration.
    /// </summary>
    public static IReadOnlyList<Type> FindAll<T>()
    {
        var key = typeof(T);
        return _findAllCache.GetOrAdd(key, _ =>
            Filter<T>()
                .OrderBy(x => x.Meta.Priority)
                .Select(x => x.Type)
                .ToList()
        );
    }

    /// <summary>
    /// Attempts to find exactly one discoverable type by name; returns null otherwise.
    /// </summary>
    public static Type? TryFindSingleByName<T>(string name)
    {
        var matches = FindManyByName<T>(name);
        return matches.Count == 1 ? matches[0] : null;
    }

    /// <summary>
    /// Enum overload for TryFindSingleByName; uses the enum's string representation.
    /// </summary>
    public static Type? TryFindSingleByName<T>(Enum name)
        => TryFindSingleByName<T>(name.ToEnumString());

    /// <summary>
    /// Retrieves all discoverable types matching the specified internal name.
    /// </summary>
    public static IReadOnlyList<Type> FindManyByName<T>(string name) =>
        Filter<T>()
            .Where(x => string.Equals(x.Meta.InternalName, name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Meta.Priority)
            .Select(x => x.Type)
            .ToList();

    public static IReadOnlyList<Type> FindManyByName<T>(Enum name) =>
        FindManyByName<T>(name.ToEnumString());

    /// <summary>
    /// Retrieves all discoverable types within the specified category.
    /// </summary>
    public static IReadOnlyList<Type> FindManyByCategory<T>(string category) =>
        Filter<T>()
            .Where(x => string.Equals(x.Meta.InternalCategory, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Meta.Priority)
            .Select(x => x.Type)
            .ToList();

    /// <summary>
    /// Overload combining name and category filters for discoverable types.
    /// </summary>
    public static IReadOnlyList<Type> FindManyByNameAndCategory<T>(string name, string category) =>
        Filter<T>()
            .Where(x =>
                string.Equals(x.Meta.InternalName, name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Meta.InternalCategory, category, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(x => x.Meta.Priority)
            .Select(x => x.Type)
            .ToList();

    /// <summary>
    /// Enum overload for FindManyByNameAndCategory; uses the enum's string representations.
    /// </summary>
    public static IReadOnlyList<Type> FindManyByNameAndCategory<T>(Enum name, Enum category) =>
        FindManyByNameAndCategory<T>(name.ToEnumString(), category.ToEnumString());

    /// <summary>
    /// Enum overload for FindManyByCategory; uses the enum's string representation.
    /// </summary>
    public static IReadOnlyList<Type> FindManyByCategory<T>(Enum category) =>
        FindManyByCategory<T>(category.ToEnumString());

    private static DiscoverableAttribute? GetMeta(Type t) =>
        t.GetCustomAttribute<DiscoverableAttribute>(inherit: false);
}




[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DiscoverableAttribute : Attribute
{
    public object? Name { get; set; }
    public object? Category { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; } = true;

    internal string InternalName =>
        Name is Enum e
            ? $"{e.GetType().FullName}.{e}"
            : Name?.ToString() ?? throw new InvalidOperationException("Discoverable.Name is null");

    internal string InternalCategory =>
        Category is Enum e
            ? $"{e.GetType().FullName}.{e}"
            : Category?.ToString() ?? throw new InvalidOperationException("Discoverable.Category is null");
}