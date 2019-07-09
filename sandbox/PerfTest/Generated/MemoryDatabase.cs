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
using TestPerfLiteDB.Tables;

namespace TestPerfLiteDB
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public TestDocTable TestDocTable { get; private set; }

        public MemoryDatabase(
            TestDocTable TestDocTable
        )
        {
            this.TestDocTable = TestDocTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver)
        {
            this.TestDocTable = ExtractTableData<TestDoc, TestDocTable>(header, headerOffset, databaseBinary, resolver, xs => new TestDocTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.TestDocTable.GetRawDataUnsafe());
            return builder;
        }
    }
}