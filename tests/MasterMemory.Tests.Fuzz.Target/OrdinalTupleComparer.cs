namespace MasterMemory.Tests.Fuzz.Target;

/// <summary>
/// The order of a composite key, as the oracles of the tests state it: member by member, a string ordinally and every
/// other type by its default comparer. Written apart from the comparer the generator emits for a composite key, which
/// is what it is checked against.
/// </summary>
public static class OrdinalTupleComparer
{
    public static IComparer<(T1, T2)> Create<T1, T2>()
    {
        return Comparer<(T1, T2)>.Create((x, y) =>
        {
            var c = Member(x.Item1, y.Item1);
            return (c != 0) ? c : Member(x.Item2, y.Item2);
        });
    }

    public static IComparer<(T1, T2, T3)> Create<T1, T2, T3>()
    {
        return Comparer<(T1, T2, T3)>.Create((x, y) =>
        {
            var c = Member(x.Item1, y.Item1);
            if (c != 0) return c;
            c = Member(x.Item2, y.Item2);
            return (c != 0) ? c : Member(x.Item3, y.Item3);
        });
    }

    public static IComparer<(T1, T2, T3, T4)> Create<T1, T2, T3, T4>()
    {
        return Comparer<(T1, T2, T3, T4)>.Create((x, y) =>
        {
            var c = Member(x.Item1, y.Item1);
            if (c != 0) return c;
            c = Member(x.Item2, y.Item2);
            if (c != 0) return c;
            c = Member(x.Item3, y.Item3);
            return (c != 0) ? c : Member(x.Item4, y.Item4);
        });
    }

    static int Member<T>(T x, T y)
    {
        return (typeof(T) == typeof(string))
            ? string.CompareOrdinal((string?)(object?)x, (string?)(object?)y)
            : Comparer<T>.Default.Compare(x, y);
    }
}
