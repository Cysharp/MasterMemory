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
        [PrimaryKey]
        public int Id { get; set; }
    }

    // Join, InnerJoin, OuterJoin, GroupJoin

        // TODO:StringComparisonOptionAttribute


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
        readonly MyClass[] data;
        readonly RangeView<MyClass> all;
        readonly RangeView<MyClass> allReverse;

        public MyClassView(MyClass[] sortedData)
        {
            this.data = sortedData;
            this.all = new RangeView<MyClass>(sortedData, 0, sortedData.Length, true);
            this.allReverse = new RangeView<MyClass>(sortedData, 0, sortedData.Length, false);
        }

        // Common Properties
        // Count, All, AllReverse, GetRawDataUnsafe

        public int Count => data.Length;
        public RangeView<MyClass> All => all;
        public RangeView<MyClass> AllReverse => allReverse;
        public MyClass[] GetRawDataUnsafe() => data;

        // Unique Key
        // TryFindByXxx, FindByXxx, FindByXxxOrDefault, FindClosestByXxx, FindRangeByXxx

        public bool TryFindById(int key, out MyClass result)
        {
            var index = BinarySearch.FindFirst(data, key, x => x.Id, (x, y) => x.CompareTo(y));
            if (index != -1)
            {
                
            }
            result = default(MyClass);
            return result;
        }

        // public 

        // Lookup Key
        // SelectByXxx, SelectClosestByXxx, SelectRangeByXxx



        // XxxAnd
        public void FindByFoo()
        {
        }

        // void FindClosest

        // public void FindRangeBy(int min, int max) // inclusive


    }



    // Annotations

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