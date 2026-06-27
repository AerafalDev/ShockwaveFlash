using System.Collections;
using System.Reflection;

namespace ShockwaveFlash.Tests;

internal static class DeepEquality
{
    public static bool Equals(object? a, object? b, string path, out string where)
    {
        where = path;

        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return Fail(path, $"null mismatch (left null: {a is null}, right null: {b is null})", out where);

        if (a is ReadOnlyMemory<byte> leftMemory && b is ReadOnlyMemory<byte> rightMemory)
            return leftMemory.Span.SequenceEqual(rightMemory.Span) || Fail(path, $"bytes differ (len {leftMemory.Length} vs {rightMemory.Length})", out where);

        if (a is string || b is string)
            return a.Equals(b) || Fail(path, $"{a} vs {b}", out where);

        if (a is IEnumerable leftSequence && b is IEnumerable rightSequence)
        {
            var left = leftSequence.Cast<object?>().ToList();
            var right = rightSequence.Cast<object?>().ToList();

            if (left.Count != right.Count)
                return Fail(path, $"count {left.Count} vs {right.Count}", out where);

            for (var i = 0; i < left.Count; i++)
                if (!Equals(left[i], right[i], $"{path}[{i}]", out where))
                    return false;

            return true;
        }

        var type = a.GetType();

        if (type != b.GetType())
            return Fail(path, $"type {type.Name} vs {b.GetType().Name}", out where);

        if (type.IsPrimitive || type.IsEnum)
            return a.Equals(b) || Fail(path, $"{a} vs {b}", out where);

        if (type.IsValueType)
            return a.Equals(b) || Fail(path, $"struct {a} vs {b}", out where);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || property.PropertyType == typeof(Type))
                continue;

            object? left;
            object? right;

            try
            {
                left = property.GetValue(a);
                right = property.GetValue(b);
            }
            catch
            {
                continue;
            }

            if (!Equals(left, right, $"{path}.{property.Name}", out where))
                return false;
        }

        return true;
    }

    private static bool Fail(string path, string message, out string where)
    {
        where = $"{path} -> {message}";
        return false;
    }
}
