namespace System;

public static class EntityExtentions
{
    public static IEnumerable<Entity> GetDescendants(this Entity e)
    {
        foreach (var child in e.Children)
        {
            yield return child;

            foreach (var descendant in child.GetDescendants())
                yield return descendant;
        }
    }

    public static T? GetDescendantOfType<T>(this Entity e) where T : Entity =>
        e.GetDescendants().OfType<T>().FirstOrDefault();

    public static bool TryGetDescendantOfType<T>(this Entity e, out T? result) where T : Entity
    {
        foreach (var descendant in e.GetDescendants())
        {
            if (descendant is T t)
            {
                result = t;
                return true;
            }
        }

        result = null;

        return false;
    }

    public static Entity GetRoot(this Entity e)
    {
        var current = e;

        while (current.Parent != null)
            current = current.Parent;

        return current;
    }

    public static bool HasChildOfType<T>(this Entity e) where T : Entity =>
        e.Children.OfType<T>().Any();

    public static void ApplyToDescendants(this Entity e, Action<Entity> action)
    {
        foreach (var descendant in e.GetDescendants())
            action(descendant);
    }

    public static IEnumerable<T> GetDescendantsOfType<T>(this Entity e) where T : Entity
    {
        foreach (var a in GetDescendants(e))
        {
            if (a is T)
                yield return (T)a;
        }
    }

    /// <summary>Returns the world‐space position by summing parent positions.</summary>
    public static Vect2 GetGlobalPosition(this Entity e)
    {
        if (e == null || e.IsExiting)
            return Vect2.Zero;

        var pos = e._position;

        if (e.IsChild && e.Parent != null)
            pos += e.Parent.GetGlobalPosition();

        // if (e.IsChild)
        // 	return e.Parent?.Position + e._position ?? e._position;
        // else
        // 	return e._position;

        return pos;
    }

    public static Vect2 GetLocalPosition(this Entity e) => e._position;

    public static Vect2 GetLocalPosition(this Entity e, Vect2 position)
    {
        if (e == null || e.IsExiting)
            return Vect2.Zero;

        return position - e.GetGlobalPosition();
    }

    public static Vect2 GetworldPosition(this Entity e, Vect2 position)
    {
        if (e == null || e.IsExiting)
            return Vect2.Zero;

        return position + e.GetGlobalPosition();
    }

    /// <summary>Yields all ancestors, from immediate parent up to root.</summary>
    public static IEnumerable<Entity> GetAncestors(this Entity e)
    {
        var current = e.Parent;

        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public static IEnumerable<T> GetAncestorsOfType<T>(this Entity e) where T : Entity
    {
        foreach (var a in GetAncestors(e))
        {
            if (a is T)
                yield return (T)a;
        }
    }

    public static void ApplyToAncestors(this Entity e, Action<Entity> action)
    {
        foreach (var ancestors in e.GetAncestors())
            action(ancestors);
    }

    /// <summary>Finds the first ancestor of type T, or null if none.</summary>
    public static T? GetAncestorOfType<T>(this Entity e) where T : Entity
        => e.GetAncestors().OfType<T>().FirstOrDefault();

    /// <summary>Tries to find the first ancestor of type T.</summary>
    public static bool TryGetAncestorOfType<T>(this Entity e, out T? result) where T : Entity
    {
        var current = e.Parent;
        while (current != null)
        {
            if (current is T t)
            {
                result = t;
                return true;
            }
            current = current.Parent;
        }

        result = null;
        return false;
    }

    public static bool HasAncestorOfType<T>(this Entity e) where T : Entity =>
        TryGetAncestorOfType<T>(e, out _);

    public static void AlignToRenderer(this Entity entity, HAlign hAlign, VAlign vAlign) =>
        AlignHelpers.AlignToRenderer(entity, hAlign, vAlign);
    public static void AlignToRenderer(this Entity entity, HAlign hAlign, VAlign vAlign, Vect2 offset) =>
        AlignHelpers.AlignToRenderer(entity, hAlign, vAlign, offset);
    public static void AlignToEntity(this Entity child, Entity parent, HAlign hAlign, VAlign vAlign) =>
        AlignHelpers.AlignToEntity(parent, child, hAlign, vAlign);
    public static void AlignToEntity(this Entity child, Entity parent, HAlign hAlign, VAlign vAlign, Vect2 offset) =>
        AlignHelpers.AlignToEntity(parent, child, hAlign, vAlign, offset);

    public static TEntity CreateInstance<TEntity>(this Entity entity, params object[] args) where TEntity : Entity =>
        InstanceHelpers.CreateInstanceFromObject<TEntity>(entity, args);
    public static bool TryCreateInstance<TEntity>(this Entity entity, out TEntity clone, params object[] args) where TEntity : Entity =>
        InstanceHelpers.TryCreateInstanceFromObject(out clone, entity, args);
}
