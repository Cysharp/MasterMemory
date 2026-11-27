using System;
using System.Collections.Generic;

namespace MasterMemory
{
    public static class DatabaseBuilderExtensions
    {
        /// <summary>
        /// The rows of a table whose row type is known only at run time, as an importer that reads the metadata has
        /// them. The generated builder knows every table and hands the rows to the Append of that one: nothing is looked
        /// up by reflection, so this is safe to call in a trimmed or Native AOT application.
        /// </summary>
        public static void AppendDynamic(this DatabaseBuilderBase builder, Type dataType, IList<object> tableData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (dataType == null) throw new ArgumentNullException(nameof(dataType));
            if (tableData == null) throw new ArgumentNullException(nameof(tableData));

            if (!builder.TryAppendDynamic(dataType, tableData))
            {
                throw new InvalidOperationException("Append(IEnumerable<DataType>) can not found. DataType:" + dataType);
            }
        }
    }
}