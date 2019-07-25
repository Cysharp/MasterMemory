using MasterMemory.Tests.TestStructures;
using MasterMemory.Tests;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.Text;
using System;
using MasterMemory.Tests.Tables;

namespace MasterMemory.Tests
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public SampleTable SampleTable { get; private set; }
        public SkillMasterTable SkillMasterTable { get; private set; }
        public TestMasterTable TestMasterTable { get; private set; }
        public UserLevelTable UserLevelTable { get; private set; }

        public MemoryDatabase(
            SampleTable SampleTable,
            SkillMasterTable SkillMasterTable,
            TestMasterTable TestMasterTable,
            UserLevelTable UserLevelTable
        )
        {
            this.SampleTable = SampleTable;
            this.SkillMasterTable = SkillMasterTable;
            this.TestMasterTable = TestMasterTable;
            this.UserLevelTable = UserLevelTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, MessagePack.IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, MessagePack.IFormatterResolver resolver)
        {
            this.SampleTable = ExtractTableData<Sample, SampleTable>(header, headerOffset, databaseBinary, resolver, xs => new SampleTable(xs));
            this.SkillMasterTable = ExtractTableData<SkillMaster, SkillMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SkillMasterTable(xs));
            this.TestMasterTable = ExtractTableData<TestMaster, TestMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new TestMasterTable(xs));
            this.UserLevelTable = ExtractTableData<UserLevel, UserLevelTable>(header, headerOffset, databaseBinary, resolver, xs => new UserLevelTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.SampleTable.GetRawDataUnsafe());
            builder.Append(this.SkillMasterTable.GetRawDataUnsafe());
            builder.Append(this.TestMasterTable.GetRawDataUnsafe());
            builder.Append(this.UserLevelTable.GetRawDataUnsafe());
            return builder;
        }
    }
}