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
                table
            
            );
        }

        public void RemoveSample(int[] keys)
        {
            var data = RemoveCore(memory.SampleTable.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new SampleTable(newData);
            memory = new MemoryDatabase(
                table
            
            );
        }

        public void Diff(Sample[] addOrReplaceData)
        {
            var data = DiffCore(memory.SampleTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new SampleTable(newData);
            memory = new MemoryDatabase(
                table
            
            );
        }

    }
}