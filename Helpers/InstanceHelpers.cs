namespace Snap.Helpers;

public static class InstanceHelpers
{
    public static bool TryCreateInstance<T>(out T instance, string name, bool ignoreCase = true, params object[] args)
    {
        instance = CreateInstance<T>(name, ignoreCase, args);

        return instance != null;
    }

    public static T CreateInstanceFromType<T>(Type type, params object[] args)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!typeof(T).IsAssignableFrom(type))
            throw new ArgumentException($"Type {type.FullName} is not assignable to {typeof(T).FullName}");

        try
        {
            var instance = Activator.CreateInstance(type, args);
            return (T)instance!;
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException($"No matching constructor found for type {type.FullName}", ex);
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException($"Constructor of type {type.FullName} threw an exception", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create instance of type {type.FullName}", ex);
        }
    }

    public static bool TryCreateInstanceFromType<T>(out T result, Type type, params object[] args)
    {
        try
        {
            result = CreateInstanceFromType<T>(type, args);
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
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
