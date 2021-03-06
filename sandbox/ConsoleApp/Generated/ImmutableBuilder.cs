// <auto-generated />
#pragma warning disable CS0105
using ConsoleApp.Tables;
using ConsoleApp;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public void ReplaceAll(System.Collections.Generic.IList<Quest> data)
        {
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new QuestTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void RemoveQuest(int[] keys)
        {
            var data = RemoveCore(memory.QuestTable.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new QuestTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void Diff(Quest[] addOrReplaceData)
        {
            var data = DiffCore(memory.QuestTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new QuestTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Item> data)
        {
            var newData = CloneAndSortBy(data, x => x.ItemId, System.Collections.Generic.Comparer<int>.Default);
            var table = new ItemTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                table,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void RemoveItem(int[] keys)
        {
            var data = RemoveCore(memory.ItemTable.GetRawDataUnsafe(), keys, x => x.ItemId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.ItemId, System.Collections.Generic.Comparer<int>.Default);
            var table = new ItemTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                table,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void Diff(Item[] addOrReplaceData)
        {
            var data = DiffCore(memory.ItemTable.GetRawDataUnsafe(), addOrReplaceData, x => x.ItemId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.ItemId, System.Collections.Generic.Comparer<int>.Default);
            var table = new ItemTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                table,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Monster> data)
        {
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                table,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void RemoveMonster(int[] keys)
        {
            var data = RemoveCore(memory.MonsterTable.GetRawDataUnsafe(), keys, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                table,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void Diff(Monster[] addOrReplaceData)
        {
            var data = DiffCore(memory.MonsterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.MonsterId, System.Collections.Generic.Comparer<int>.Default);
            var table = new MonsterTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                table,
                memory.PersonTable,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Person> data)
        {
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                table,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void RemovePerson(int[] keys)
        {
            var data = RemoveCore(memory.PersonTable.GetRawDataUnsafe(), keys, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                table,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void Diff(Person[] addOrReplaceData)
        {
            var data = DiffCore(memory.PersonTable.GetRawDataUnsafe(), addOrReplaceData, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.PersonId, System.Collections.Generic.Comparer<int>.Default);
            var table = new PersonTable(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                table,
                memory.Test1Table,
                memory.Test2Table
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Test1> data)
        {
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test1Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                table,
                memory.Test2Table
            
            );
        }

        public void RemoveTest1(int[] keys)
        {
            var data = RemoveCore(memory.Test1Table.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test1Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                table,
                memory.Test2Table
            
            );
        }

        public void Diff(Test1[] addOrReplaceData)
        {
            var data = DiffCore(memory.Test1Table.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test1Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                table,
                memory.Test2Table
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<Test2> data)
        {
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test2Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                table
            
            );
        }

        public void RemoveTest2(int[] keys)
        {
            var data = RemoveCore(memory.Test2Table.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test2Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                table
            
            );
        }

        public void Diff(Test2[] addOrReplaceData)
        {
            var data = DiffCore(memory.Test2Table.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new Test2Table(newData);
            memory = new MemoryDatabase(
                memory.QuestTable,
                memory.ItemTable,
                memory.MonsterTable,
                memory.PersonTable,
                memory.Test1Table,
                table
            
            );
        }

    }
}