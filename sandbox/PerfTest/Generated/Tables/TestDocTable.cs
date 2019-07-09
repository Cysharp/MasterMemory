using LiteDB;
using MasterMemory.Annotations;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using TestPerfLiteDB;
using ZeroFormatter;

namespace TestPerfLiteDB.Tables
{
   public sealed partial class TestDocTable : TableBase<TestDoc>
   {
        readonly Func<TestDoc, int> primaryIndexSelector;


        public TestDocTable(TestDoc[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.id;
        }

        public TestDoc FindByid(int key)
        {
            return FindUniqueCoreInt(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key);
        }

        public TestDoc FindClosestByid(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<TestDoc> FindRangeByid(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

    }
}