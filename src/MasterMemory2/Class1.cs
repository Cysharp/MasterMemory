using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MasterMemory
{

    [MemoryTable("foo")]
    public class MyClass
    {
        [PrimaryKey, LookupView]
        public int Foo { get; private set; }
    }

    // Join, InnerJoin, OuterJoin, GroupJoin




    public class MemoryTableAttribute : Attribute
    {
        public MemoryTableAttribute(string tableName)
        {
        }
    }

    public class LookupViewAttribute : Attribute
    {

    }



    public interface IMemoryDatabase
    {
        T GetView<T>();
    }

    // IView
    // void FindByIndex1();



    public partial class MemoryDatabase : IMemoryDatabase
    {
        Dictionary<string, object> viewDictionary;
        MyClassView myClassView;

        public void Open()
        {
            // Streaming Read

            // A...
            // B...
            // C...
            // D...

            // Build
        }

        public T GetView<T>()
        {
            return Cache<T>.Instance;
        }

        public MyClassView GetMyClassView()
        {
            return myClassView; // 
        }

        // void ToImmutableBuilder()

        static class Cache<T>
        {
            public static T Instance;
        }
    }

    public class ImmutableBuilder
    {
        public ImmutableBuilder(MemoryDatabase memory)
        {

        }

        void Replace()
        {
        }

        MemoryDatabase Build()
        {
            throw new NotImplementedException();
        }
    }

    public partial class DatabaseBuilder
    {
        byte[] byteBuffer;

        void Append(IEnumerable<MyClass> data)
        {
            // Get Db Name.
            // Automaticaly String.Intern(
            // String.Intern(
            // Sort By Primary Index.
            // Create Secondary Index.
        }

        void Build()
        {
            // File/
        }

        void SaveToFile()
        {
        }

        void WriteToStream()
        {
        }

        ArraySegment<byte> GetRawBuffer()
        {
            throw new NotImplementedException();
        }
    }


    public partial class MyClassView
    {
        // RangeView
        // RangeView

        public void GetRawDataUnsafe()
        {
            throw new NotImplementedException();
        }

        public void GetRawPrimaryIndexUnsafe() // SecondaryIndex1, SecondaryIndex2, etc...
        {
            // new Dictionary<int,int>().
            throw new NotImplementedException();
        }

        // Count
        public int Count => throw new NotImplementedException();

        // (use rangeview instead?)
        public IReadOnlyList<int> All()
        {
            throw new NotImplementedException();
        }


        // Common Properties
        // Count, All, AllReverse

        // Unique Key
        // TryFindByXxx, FindByXxx, FindByXxxOrDefault, FindClosestByXxx, FindRangeByXxx

        // Lookup Key
        // SelectByXxx, SelectClosestByXxx, SelectRangeByXxx



        // XxxAnd
        public void FindByFoo()
        {
        }

        // void FindClosest

        // public void FindRangeBy(int min, int max) // inclusive


    }




    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute(int keyOrder = 0)
        {

        }
    }



    public class SecondaryKeyAttribute : Attribute
    {
        public SecondaryKeyAttribute(int indexNo, int keyOrder = 0)
        {

        }
    }
}