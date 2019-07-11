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

        public void ReplaceAll(System.Collections.Generic.IList<Monster> data)
        {
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.PersonTable
            
            );
        }

        public void RemoveMonster(int[] keys)
        {
            var data = RemoveCore(memory.MonsterTable.GetRawDataUnsafe(), keys, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.PersonTable
            
            );
        }

        public void Diff(Monster[] addOrReplaceData)
        {
            var data = DiffCore(memory.MonsterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.PersonTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Person> data)
        {
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.MonsterTable,
                table
            
            );
        }

        public void RemovePerson(int[] keys)
        {
            var data = RemoveCore(memory.PersonTable.GetRawDataUnsafe(), keys, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.MonsterTable,
                table
            
            );
        }

        public void Diff(Person[] addOrReplaceData)
        {
            var data = DiffCore(memory.PersonTable.GetRawDataUnsafe(), addOrReplaceData, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.MonsterTable,
                table
            
            );
        }

    }
}