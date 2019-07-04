using MasterMemory.Annotations;
using MessagePack;
using System;
using System.Collections.Generic;

namespace MasterMemory
{

    [MemoryTable("foo"), MessagePackObject(true)]
    public class MyClass
    {
        [PrimaryKey]
        public int Id { get; set; }

        [SecondaryKey(0), NonUnique]
        public DateTime Timestamp { get; set; }
    }


    // Generate DatabaseBuilder

    public sealed class DatabaseBuilder : DatabaseBuilderBase
    {
        public void Append(IEnumerable<MyClass> dataSource)
        {
            AppendCore(dataSource, x => x.Id);
        }

        // And many Append methods...
    }

    // Generate MemoryDatabase

    public sealed class MemoryDatabase : MemoryDatabaseBase
    {
        public MemoryDatabase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        public MyClassTable MyClassTable { get; private set; }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver)
        {
            this.MyClassTable = ExtractTableData<MyClass, MyClassTable>(header, headerOffset, databaseBinary, resolver, xs => new MyClassTable(xs));
            // And many Init members.
        }
    }

    // Generate Table

    public partial class MyClassTable : TableBase<MyClass>
    {
        readonly DateTime[] secondaryIndex0;

        readonly Func<MyClass, int> primaryIndexSelector;
        readonly Func<DateTime, DateTime> secondaryIndex0IdentitySelector;

        public MyClassTable(MyClass[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Id;
            this.secondaryIndex0IdentitySelector = x => x;

            // Generate
            this.secondaryIndex0 = new DateTime[sortedData.Length];
            for (int i = 0; i < sortedData.Length; i++)
            {
                this.secondaryIndex0[i] = sortedData[i].Timestamp;
                // ..Indexes...
            }

            // Sort
            Array.Sort(this.secondaryIndex0);
        }


        public MyClass FindById(int key)
        {
            return FindUniqueCore(data, primaryIndexSelector, key);
        }

        public MyClass FindClosestById(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, key, selectLower);
        }

        public RangeView<MyClass> FindRangeById(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, min, max, ascendant);
        }

        public RangeView<MyClass> FindByTimestamp(DateTime key, bool ascendant = true)
        {
            return FindManyCore(secondaryIndex0, secondaryIndex0IdentitySelector, key, ascendant);
        }

        public RangeView<MyClass> FindClosestByTimestamp(DateTime key, bool selectLower = true, bool ascendant = true)
        {
            return FindManyClosestCore(secondaryIndex0, secondaryIndex0IdentitySelector, key, selectLower, ascendant);
        }

        public RangeView<MyClass> FindRangeByTimestamp(DateTime min, DateTime max, bool ascendant = true)
        {
            return FindManyRangeCore(secondaryIndex0, secondaryIndex0IdentitySelector, min, max, ascendant);
        }
    }
}