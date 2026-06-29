using System;
using System.Collections;
using System.Collections.Generic;

namespace ShockwaveFlash.SourceGenerators;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly T[]? _items;

    public int Count =>
        _items?.Length ?? 0;

    public T this[int index] =>
        _items![index];

    public EquatableArray(T[] items)
    {
        _items = items;
    }

    public bool Equals(EquatableArray<T> other)
    {
        var left = _items;
        var right = other._items;

        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null || left.Length != right.Length)
            return false;

        for (var i = 0; i < left.Length; i++)
        {
            if (!left[i].Equals(right[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }


    public override int GetHashCode()
    {
        if (_items is null)
            return 0;

        unchecked
        {
            var hash = 17;

            foreach (var item in _items)
                hash = (hash * 31) + (item?.GetHashCode() ?? 0);

            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items ?? [])
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}
