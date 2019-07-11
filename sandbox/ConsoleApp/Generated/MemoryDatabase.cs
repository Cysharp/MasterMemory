using ConsoleApp;
using MasterMemory.Annotations;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System;
using ConsoleApp.Tables;

namespace ConsoleApp
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public MonsterTable MonsterTable { get; private set; }
        public PersonTable PersonTable { get; private set; }

        public MemoryDatabase(
            MonsterTable MonsterTable,
            PersonTable PersonTable
        )
        {
            this.MonsterTable = MonsterTable;
            this.PersonTable = PersonTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, MessagePack.IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, MessagePack.IFormatterResolver resolver)
        {
            this.MonsterTable = ExtractTableData<Monster, MonsterTable>(header, headerOffset, databaseBinary, resolver, xs => new MonsterTable(xs));
            this.PersonTable = ExtractTableData<Person, PersonTable>(header, headerOffset, databaseBinary, resolver, xs => new PersonTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.MonsterTable.GetRawDataUnsafe());
            builder.Append(this.PersonTable.GetRawDataUnsafe());
            return builder;
        }
    }
}