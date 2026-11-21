using System.Collections;
using System.Runtime.CompilerServices;

namespace MasterMemory;

/// <summary>An array compared element by element, so a record that holds one is compared by value.</summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    readonly T[]? array;

    public EquatableArray() // for collection literal []
    {
        array = [];
    }

    public EquatableArray(T[] array)
    {
        this.array = array;
    }

    public static implicit operator EquatableArray<T>(T[] array)
    {
        return new EquatableArray<T>(array);
    }

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref array![index];
    }

    // default(EquatableArray<T>) is an empty array
    public int Length => (array == null) ? 0 : array.Length;

    public ReadOnlySpan<T> AsSpan()
    {
        return array.AsSpan();
    }

    public T[] ToArray()
    {
        return AsSpan().ToArray();
    }

    public ReadOnlySpan<T>.Enumerator GetEnumerator()
    {
        return AsSpan().GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return (array ?? []).AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (array ?? []).AsEnumerable().GetEnumerator();
    }

    public bool Equals(EquatableArray<T> other)
    {
        return AsSpan().SequenceEqual(other.AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    // equal arrays must hash alike: the struct's default would hash the reference of the array
    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in AsSpan())
        {
            hash = unchecked((hash * 31) + (item?.GetHashCode() ?? 0));
        }
        return hash;
    }
}
