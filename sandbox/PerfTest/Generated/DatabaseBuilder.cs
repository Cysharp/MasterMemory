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
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<TestDoc> dataSource)
        {
            AppendCore(dataSource, x => x.id, System.Collections.Generic.Comparer<int>.Default);
			return this;
        }

    }
}