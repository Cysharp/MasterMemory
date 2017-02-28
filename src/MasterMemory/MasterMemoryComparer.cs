using System.Collections.Generic;

namespace MasterMemory
{
    /// <summary>
    /// Replacable Comaprer`T`.Default.
    /// </summary>
    public static class MasterMemoryComparer<T>
    {
        static IComparer<T> _default;

        public static IComparer<T> Default
        {
            get
            {
                return _default ?? (_default = Comparer<T>.Default);
            }
            set
            {
                _default = value;
            }
        }

        static IComparer<T>[] _array;

        public static IComparer<T>[] DefaultArray
        {
            get
            {
                return _array ?? (_array = new[] { Default });
            }
            set
            {
                _array = value;
            }
        }
    }
}
