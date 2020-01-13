using System;

namespace MasterMemory
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MemoryTableAttribute : Attribute
    {
        public string TableName { get; }

        public MemoryTableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        public int KeyOrder { get; }

        public PrimaryKeyAttribute(int keyOrder = 0)
        {
            this.KeyOrder = keyOrder;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SecondaryKeyAttribute : Attribute
    {
        public int IndexNo { get; }
        public int KeyOrder { get; }

        public SecondaryKeyAttribute(int indexNo, int keyOrder = 0)
        {
            this.IndexNo = indexNo;
            this.KeyOrder = keyOrder;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NonUniqueAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StringComparisonOptionAttribute : Attribute
    {
        public StringComparison StringComparison { get; }

        public StringComparisonOptionAttribute(StringComparison stringComparison)
        {
            this.StringComparison = stringComparison;
        }
    }
}