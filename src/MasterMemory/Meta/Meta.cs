using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MasterMemory.Meta
{
    public class MetaDatabase
    {
        IDictionary<string, MetaTable> tableInfos;

        public MetaDatabase(IDictionary<string, MetaTable> tableInfos)
        {
            this.tableInfos = tableInfos;
        }

        public int Count => tableInfos.Count;

        public IEnumerable<MetaTable> GetTableInfos()
        {
            foreach (var item in tableInfos.Values)
            {
                yield return item;
            }
        }

        public MetaTable GetTableInfo(string tableName)
        {
            return tableInfos.TryGetValue(tableName, out var table)
                ? table
                : null;
        }
    }

    public class MetaTable
    {
        public Type DataType { get; }
        public Type TableType { get; }
        public string TableName { get; }
        public IReadOnlyList<MetaProperty> Properties { get; }
        public IReadOnlyList<MetaIndex> Indexes { get; }

        public MetaTable(Type dataType, Type tableType, string tableName, IReadOnlyList<MetaProperty> properties, IReadOnlyList<MetaIndex> Indexes)
        {
            this.DataType = dataType;
            this.TableType = tableType;
            this.TableName = tableName;
            this.Properties = properties;
            this.Indexes = Indexes;
        }

        public override string ToString()
        {
            return TableName;
        }
    }

    public class MetaProperty
    {
        public PropertyInfo PropertyInfo { get; }

        public string Name => PropertyInfo.Name;
        public string NameLowerCamel => ToCamelCase(PropertyInfo.Name);
        public string NameSnakeCase => ToSnakeCase(PropertyInfo.Name);

        public MetaProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// MyProperty -> myProperty
        /// </summary>
        static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsLower(s, 0))
            {
                return s;
            }

            var array = s.ToCharArray();
            array[0] = char.ToLowerInvariant(array[0]);
            return new string(array);
        }

        /// <summary>
        /// MyProperty -> my_property
        /// </summary>
        static string ToSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];

                if (Char.IsUpper(c))
                {
                    // first
                    if (i == 0)
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                    else if (char.IsUpper(s[i - 1])) // WriteIO => write_io
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        sb.Append("_");
                        sb.Append(char.ToLowerInvariant(c));
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }

    public class MetaIndex
    {
        public IReadOnlyList<PropertyInfo> IndexProperties { get; }
        public bool IsPrimaryIndex { get; }
        public bool IsUnique { get; }
        public System.Collections.IComparer Comparer { get; }
        public bool IsReturnRangeValue => IndexProperties.Count != 1;

        public MetaIndex(IReadOnlyList<PropertyInfo> indexProperties, bool isPrimaryIndex, bool isUnique, System.Collections.IComparer comparer)
        {
            IndexProperties = indexProperties;
            IsPrimaryIndex = isPrimaryIndex;
            IsUnique = isUnique;
            Comparer = comparer;
        }

        public override string ToString()
        {
            return string.Join(", ", IndexProperties.Select(x => x.Name));
        }
    }
}