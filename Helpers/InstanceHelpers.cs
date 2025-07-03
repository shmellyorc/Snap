namespace Snap.Helpers;

public static class InstanceHelpers
{
    public static bool TryCreateInstance<T>(out T instance, string name, bool ignoreCase = true, params object[] args)
    {
        instance = CreateInstance<T>(name, ignoreCase, args);

        return instance != null;
    }

    public static T CreateInstance<T>(string name, bool ignoreCase, params object[] args)
    {
        if (name.IsEmpty())
            return default!;

        var ap = AppDomain.CurrentDomain.GetAssemblies();
        var ignoreType = ignoreCase
             ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        for (int i = ap.Length - 1; i >= 0; i--)
        {
            var asm = ap[i];
            var foundType = asm.GetType(name, false, ignoreCase);

            if (foundType == null)
            {
                foundType = asm.GetTypes()
                    .FirstOrDefault(x => string.Equals(x.Name, name, ignoreType));
            }

            if (foundType == null)
                continue;

            if (!typeof(T).IsAssignableFrom(foundType))
                continue;

            return (T)Activator.CreateInstance(foundType, args)!;
        }

        return default;
    }

    public static T CreateInstanceFromObject<T>(object obj, params object[] args) =>
        CreateInstance<T>(obj.GetType().Name, true, args);

    public static bool TryCreateInstanceFromObject<T>(out T instance, object obj, params object[] args)
    {
        instance = CreateInstance<T>(obj.GetType().Name, true, args);

        return instance != null;
    }
}
