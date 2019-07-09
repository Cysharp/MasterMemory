using MasterMemory.Annotations;
using MasterMemory.Tests;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using MasterMemory.Tests.Tables;

namespace MasterMemory.Tests
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public SampleTable SampleTable { get; private set; }

        public MemoryDatabase(
            SampleTable SampleTable
        )
        {
            this.SampleTable = SampleTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver)
        {
            this.SampleTable = ExtractTableData<Sample, SampleTable>(header, headerOffset, databaseBinary, resolver, xs => new SampleTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.SampleTable.GetRawDataUnsafe());
            return builder;
        }
    }
}