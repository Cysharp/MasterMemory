using MasterMemory.Annotations;
using MessagePack;
using System;
using System.Collections.Generic;

namespace MasterMemory
{

    [MemoryTable("foo"), MessagePackObject(true)]
    public class MyClass
    {
        [PrimaryKey(999)]
        [SecondaryKey(0, 123), NonUnique]
        public int Id { get; set; }

        [SecondaryKey(0), NonUnique]
        [SecondaryKey(1, 10), NonUnique]
        public DateTime Timestamp { get; set; }

        [SecondaryKey(3), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string Nano { get; set; }
    }


    // Generate DatabaseBuilder

    public sealed class DatabaseBuilder : DatabaseBuilderBase
    {
        public void Append(System.Collections.Generic.IEnumerable<MyClass> dataSource)
        {
            AppendCore(dataSource, x => x.Id);
        }

    }

    // Generate MemoryDatabase

    public sealed class MemoryDatabase : MemoryDatabaseBase
    {
        public MemoryDatabase(
        MyClassTable MyClassTable
        )
        {
            this.MyClassTable = MyClassTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        public MyClassTable MyClassTable { get; private set; }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver)
        {
            this.MyClassTable = ExtractTableData<MyClass, MyClassTable>(header, headerOffset, databaseBinary, resolver, xs => new MyClassTable(xs));
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(MyClassTable.GetRawDataUnsafe());

            return builder;
        }
    }

    // Generate Table
    public sealed partial class MyClassTable : TableBase<MyClass>
    {
        readonly Func<MyClass, int> primaryIndexSelector;

        readonly MyClass[] secondaryIndex0;
        readonly Func<MyClass, (DateTime Timestamp, int Id)> secondaryIndex0Selector;
        readonly MyClass[] secondaryIndex1;
        readonly Func<MyClass, DateTime> secondaryIndex1Selector;
        readonly MyClass[] secondaryIndex3;
        readonly Func<MyClass, string> secondaryIndex3Selector;

        public MyClassTable(MyClass[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Id;
            this.secondaryIndex0Selector = x => (x.Timestamp, x.Id);
            this.secondaryIndex0 = CloneAndSortBy(this.secondaryIndex0Selector, Comparer<(DateTime Timestamp, int Id)>.Default);
            this.secondaryIndex1Selector = x => x.Timestamp;
            this.secondaryIndex1 = CloneAndSortBy(this.secondaryIndex1Selector, Comparer<DateTime>.Default);
            this.secondaryIndex3Selector = x => x.Nano;
            this.secondaryIndex3 = CloneAndSortBy(this.secondaryIndex3Selector, StringComparer.OrdinalIgnoreCase); // TODO:Call StringComparison.
        }

        public MyClass FindById(int key)
        {
            return FindUniqueCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key);
        }

        public MyClass FindClosestById(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<MyClass> FindRangeById(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

        public RangeView<MyClass> FindByTimestampAndId((DateTime Timestamp, int Id) key)
        {
            return FindManyCore(data, secondaryIndex0Selector, System.Collections.Generic.Comparer<(DateTime Timestamp, int Id)>.Default, key);
        }

        public RangeView<MyClass> FindClosestByTimestampAndId((DateTime Timestamp, int Id) key, bool selectLower = true)
        {
            return FindManyClosestCore(data, secondaryIndex0Selector, System.Collections.Generic.Comparer<(DateTime Timestamp, int Id)>.Default, key, selectLower);
        }

        public RangeView<MyClass> FindRangeByTimestampAndId((DateTime Timestamp, int Id) min, (DateTime Timestamp, int Id) max, bool ascendant = true)
        {
            return FindManyRangeCore(data, secondaryIndex0Selector, System.Collections.Generic.Comparer<(DateTime Timestamp, int Id)>.Default, min, max, ascendant);
        }

        public RangeView<MyClass> FindByTimestamp(DateTime key)
        {
            return FindManyCore(data, secondaryIndex1Selector, System.Collections.Generic.Comparer<DateTime>.Default, key);
        }

        public RangeView<MyClass> FindClosestByTimestamp(DateTime key, bool selectLower = true)
        {
            return FindManyClosestCore(data, secondaryIndex1Selector, System.Collections.Generic.Comparer<DateTime>.Default, key, selectLower);
        }

        public RangeView<MyClass> FindRangeByTimestamp(DateTime min, DateTime max, bool ascendant = true)
        {
            return FindManyRangeCore(data, secondaryIndex1Selector, System.Collections.Generic.Comparer<DateTime>.Default, min, max, ascendant);
        }

        public MyClass FindByNano(string key)
        {
            return FindUniqueCore(data, secondaryIndex3Selector, System.StringComparer.OrdinalIgnoreCase, key);
        }

        public MyClass FindClosestByNano(string key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, secondaryIndex3Selector, System.StringComparer.OrdinalIgnoreCase, key, selectLower);
        }

        public RangeView<MyClass> FindRangeByNano(string min, string max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, secondaryIndex3Selector, System.StringComparer.OrdinalIgnoreCase, min, max, ascendant);
        }

    }
}