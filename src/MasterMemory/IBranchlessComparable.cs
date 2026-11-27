using System;

namespace MasterMemory
{
    /// <summary>
    /// A lookup key that can also tell where it stands to a row without a conditional jump. The generated code
    /// implements it for a key whose difference to another is exact arithmetic (an integer of 32 bits or less), and
    /// the search then halves its range by masking instead of jumping.
    /// </summary>
    public interface IBranchlessComparable<TElement> : IComparable<TElement>
    {
        /// <summary>All bits set (-1) when the key is at or after <paramref name="other"/>, 0 when it is before.</summary>
        int AtOrAfterMask(TElement other);
    }
}