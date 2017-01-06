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
    }
}
