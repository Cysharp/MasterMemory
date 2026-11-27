using System;
using System.Collections.Generic;

namespace MasterMemory.Internal
{
    /// <summary>
    /// The sort every table order is made by: the rows of a table in a builder, and each secondary index of a table.
    /// </summary>
    /// <remarks>
    /// It is a merge sort, so keys that compare equal stay in the order they came in, which Array.Sort does not
    /// promise: rows that share a key of a secondary index are in the order of the primary key, and rows that share a
    /// primary key are in the order they were given to the builder, on every runtime.
    ///
    /// What is moved while sorting is the position of each key, an int, and neither the key nor the row. Comparing
    /// rows would read one object after another, wherever each is in memory, where the keys lie in one array; and
    /// moving the keys, or the rows, would write a reference for each move wherever a key holds a string, which costs
    /// more than writing an int. Measured on x64 (.NET 9) against Array.Sort with the keys beside the rows, a table
    /// of nine indexes is made in three quarters of the time up to 1,000 rows and in nine tenths of it at 100,000.
    /// Index by index, a key of numbers, a composite key and a string key compared without case take two thirds to
    /// nine tenths of the time, and an ordinal string key that many rows share takes a twentieth more.
    /// </remarks>
    internal static class StableSort
    {
        // runs of this many keys are put in order by insertion, and merged two by two from there
        const int RunLength = 16;

        /// <summary>
        /// Where each key is, in the order of the keys: buffer[result] is the position of the first key,
        /// buffer[result + 1] that of the next, keys.Length of them. Keys that compare equal are in the order of their
        /// positions. Negative when the keys are in order as they are, which takes one pass to find and is what a
        /// table read from a database, or rows given in the order of their ids, come as.
        /// </summary>
        /// <remarks>
        /// <paramref name="buffer"/> is what the sort works in, twice the keys long. It is made here when it is null
        /// or too short, and can be handed to the next sort: the indexes of one table are sorted one after another in
        /// one buffer, where a buffer for each was a third more garbage for a table to load with.
        /// A comparison that contradicts itself gives the keys in some order, and no exception.
        /// </remarks>
        public static int Positions<TKey, TComparer>(TKey[] keys, TComparer comparer, ref int[]? buffer)
            where TComparer : IComparer<TKey>
        {
            // How the comparer is called decides what the sort costs, in time and in code. A comparer that is a struct
            // is compiled into the sort: fastest, and a sort of its own for each such struct, compiled when a table
            // first needs it (a third of a millisecond under a JIT) or ahead of time (code for each). The generated
            // code has one for a composite key and an enum key, which have no comparer to share. A comparer that is an
            // object is called through its interface, and all keys of one type share one sort: what a string key is
            // sorted by. The default comparer, the order of a key of one number, is called through a struct of this
            // class: one sort for each key type, and the comparison of the type is compiled into it where the
            // runtime can do that.
            if (!typeof(TComparer).IsValueType && ReferenceEquals(comparer, Comparer<TKey>.Default))
            {
                return PositionsCore(keys, default(DefaultOrder<TKey>), ref buffer);
            }

            return PositionsCore(keys, comparer, ref buffer);
        }

        readonly struct DefaultOrder<TKey> : IComparer<TKey>
        {
            public int Compare(TKey x, TKey y) => Comparer<TKey>.Default.Compare(x, y);
        }

        static int PositionsCore<TKey, TComparer>(TKey[] keys, TComparer comparer, ref int[]? buffer)
            where TComparer : IComparer<TKey>
        {
            var length = keys.Length;

            var inOrder = true;
            for (var i = 1; i < length; i++)
            {
                if (comparer.Compare(keys[i - 1], keys[i]) > 0)
                {
                    inOrder = false;
                    break;
                }
            }
            if (inOrder) return -1;

            if (buffer == null || buffer.Length < checked(length * 2))
            {
                buffer = new int[length * 2];
            }

            // the positions are in one half of the buffer, and each round of merges writes them into the other
            var positions = buffer;
            for (var i = 0; i < length; i++)
            {
                positions[i] = i;
            }

            for (var start = 0; start < length; start += RunLength)
            {
                var end = Math.Min(start + RunLength, length);
                for (var i = start + 1; i < end; i++)
                {
                    var position = positions[i];
                    var j = i - 1;
                    // only past a key that is after this one: an equal key before it stays before it
                    while (j >= start && comparer.Compare(keys[positions[j]], keys[position]) > 0)
                    {
                        positions[j + 1] = positions[j];
                        j--;
                    }
                    positions[j + 1] = position;
                }
            }

            var from = 0;
            var to = length;
            for (var width = RunLength; width < length; width *= 2)
            {
                for (var left = 0; left < length; left += width * 2)
                {
                    var middle = Math.Min(left + width, length);
                    var right = Math.Min(left + width * 2, length);

                    // one run alone, or two that are in order one after the other: nothing to merge
                    if (middle == right || comparer.Compare(keys[positions[from + middle - 1]], keys[positions[from + middle]]) <= 0)
                    {
                        Array.Copy(positions, from + left, positions, to + left, right - left);
                        continue;
                    }

                    var i = from + left;
                    var iEnd = from + middle;
                    var j = iEnd;
                    var jEnd = from + right;
                    var k = to + left;
                    while (i < iEnd && j < jEnd)
                    {
                        // of two equal keys the one of the left run, which came first
                        var a = positions[i];
                        var b = positions[j];
                        if (comparer.Compare(keys[a], keys[b]) <= 0)
                        {
                            positions[k++] = a;
                            i++;
                        }
                        else
                        {
                            positions[k++] = b;
                            j++;
                        }
                    }
                    if (i < iEnd) Array.Copy(positions, i, positions, k, iEnd - i);
                    if (j < jEnd) Array.Copy(positions, j, positions, k, jEnd - j);
                }

                var swap = from;
                from = to;
                to = swap;
            }

            return from;
        }

        /// <summary>
        /// The rows in a new array, in the order of their keys; rows of equal keys in the order they are in
        /// <paramref name="rows"/>. keys[i] is the key of rows[i].
        /// </summary>
        public static TRow[] Sort<TRow, TKey, TComparer>(TRow[] rows, TKey[] keys, TComparer comparer, ref int[]? buffer)
            where TComparer : IComparer<TKey>
        {
            var sorted = new TRow[rows.Length];
            var first = Positions(keys, comparer, ref buffer);
            if (first < 0)
            {
                Array.Copy(rows, sorted, rows.Length);
            }
            else
            {
                var positions = buffer!;
                for (var i = 0; i < sorted.Length; i++)
                {
                    sorted[i] = rows[positions[first + i]];
                }
            }
            return sorted;
        }
    }
}
