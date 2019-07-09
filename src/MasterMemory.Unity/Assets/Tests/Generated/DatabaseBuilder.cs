using MasterMemory.Annotations;
using MasterMemory.Tests;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using MasterMemory.Tests.Tables;

namespace MasterMemory.Tests
{
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Sample> dataSource)
        {
            AppendCore(dataSource, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
			return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SkillMaster> dataSource)
        {
            AppendCore(dataSource, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
			return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<UserLevel> dataSource)
        {
            AppendCore(dataSource, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
			return this;
        }

    }
}