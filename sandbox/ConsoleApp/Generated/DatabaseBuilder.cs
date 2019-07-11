using ConsoleApp;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System;
using ConsoleApp.Tables;

namespace ConsoleApp
{
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Monster> dataSource)
        {
            AppendCore(dataSource, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
			return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Person> dataSource)
        {
            AppendCore(dataSource, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
			return this;
        }

    }
}