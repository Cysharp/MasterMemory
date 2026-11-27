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
                // every generated table is a TableBase<T>, which says what its rows are without reflection
                if (!(table is IValidationTable validationTable))
                {
                    throw new ArgumentException("A table of a database is a TableBase<T>, this is not one: " + (table == null ? "null" : table.GetType().FullName), nameof(tables));
                }

                dataTables.Add(validationTable.ElementType, validationTable.Rows);
            }
        }

        internal IReadOnlyList<T> GetTable<T>()
        {
            if (!dataTables.TryGetValue(typeof(T), out var table))
            {
                throw new InvalidOperationException("Can not create validator in " + typeof(T).FullName);
            }
            var data = table as IReadOnlyList<T>;
            return data!;
        }
    }

    /// <summary>What the validation reads of a table, whatever its row type: implemented by TableBase&lt;T&gt;.</summary>
    internal interface IValidationTable
    {
        Type ElementType { get; }

        /// <summary>An IReadOnlyList&lt;T&gt; of every row, in the order of the primary key.</summary>
        object Rows { get; }
    }
}
