using MasterMemory.Annotations;
using MasterMemory.Tests;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using MasterMemory.Tests.Tables;

namespace MasterMemory.Tests
{
   public sealed class ImmutableBuilder : ImmutableBuilderBase
   {
        MemoryDatabase memory;

        public ImmutableBuilder(MemoryDatabase memory)
        {
            this.memory = memory;
        }

        public MemoryDatabase Build()
        {
            return memory;
        }

        public void ReplaceAll(System.Collections.Generic.IList<Sample> data)
        {
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new SampleTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SkillMasterTable,
                memory.UserLevelTable
            
            );
        }

        public void RemoveSample(int[] keys)
        {
            var data = RemoveCore(memory.SampleTable.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new SampleTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SkillMasterTable,
                memory.UserLevelTable
            
            );
        }

        public void Diff(Sample[] addOrReplaceData)
        {
            var data = DiffCore(memory.SampleTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new SampleTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SkillMasterTable,
                memory.UserLevelTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SkillMaster> data)
        {
            var newData = CloneAndSortBy(data, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
            var table = new SkillMasterTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                table,
                memory.UserLevelTable
            
            );
        }

        public void RemoveSkillMaster((int SkillId, int SkillLevel)[] keys)
        {
            var data = RemoveCore(memory.SkillMasterTable.GetRawDataUnsafe(), keys, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
            var newData = CloneAndSortBy(data, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
            var table = new SkillMasterTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                table,
                memory.UserLevelTable
            
            );
        }

        public void Diff(SkillMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SkillMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
            var newData = CloneAndSortBy(data, x => (x.SkillId, x.SkillLevel), System.Collections.Generic.Comparer<(int SkillId, int SkillLevel)>.Default);
            var table = new SkillMasterTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                table,
                memory.UserLevelTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<UserLevel> data)
        {
            var newData = CloneAndSortBy(data, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
            var table = new UserLevelTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                memory.SkillMasterTable,
                table
            
            );
        }

        public void RemoveUserLevel(int[] keys)
        {
            var data = RemoveCore(memory.UserLevelTable.GetRawDataUnsafe(), keys, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
            var table = new UserLevelTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                memory.SkillMasterTable,
                table
            
            );
        }

        public void Diff(UserLevel[] addOrReplaceData)
        {
            var data = DiffCore(memory.UserLevelTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Level, System.Collections.Generic.Comparer<int>.Default);
            var table = new UserLevelTable(newData);
            memory = new MemoryDatabase(
                memory.SampleTable,
                memory.SkillMasterTable,
                table
            
            );
        }

    }
}