using System;
using System.Collections.Generic;

namespace MasterMemory.Validation
{
    public class ValidationDatabase
    {
        // {Type, IReadOnlyList<T> }
        readonly Dictionary<Type, object> dataTables = new Dictionary<Type, object>();

        public ValidationDatabase(IEnumerable<object> tables)
        {
            foreach (var table in tables)
            {
                // TableBase<T>
                var baseType = table.GetType().BaseType;

                // RangeView<T>
                var rangeViewAll = baseType.GetProperty("All").GetGetMethod().Invoke(table, null);

                var elementType = baseType.GetGenericArguments()[0];
                dataTables.Add(elementType, rangeViewAll);
            }
        }

        internal IReadOnlyList<T> GetTable<T>()
        {
            if (!dataTables.TryGetValue(typeof(T), out var table))
            {
                throw new InvalidOperationException("Can not create validator in " + typeof(T).FullName);
            }
            var data = table as IReadOnlyList<T>;
            return data;
        }
    }
}
