using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory
{
    // Memory Layout:
    // [beginMemoryOffset:int][(key:string,memoryOffset:index)][memory...]

    public class Database
    {
        public int MemoryCount { get; private set; }
        public int BinarySize { get; private set; }

        public void GetMemory<T, TIndex>(string memoryKey, Func<T, TIndex> indexSelector)
        {
        }

        public static Database Open(string filePath)
        {
            throw new NotImplementedException();
        }

        public void Save(string filePath)
        {
        }
    }

    // 8 secondary index.
    public class DatabaseBuilder
    {
        public void Add<T, TPrimaryIndex>(string key, IEnumerable<T> datasource, Func<T, TPrimaryIndex> primaryIndexSelector)
        {
        }

        public void Add<T, TPrimaryIndex, TSecondaryIndex1>(string key, IEnumerable<T> datasource, Func<T, TPrimaryIndex> primaryIndexSelector, Func<T, TSecondaryIndex1> secondaryIndexSelector1)
        {
        }

        public Database Build()
        {
            throw new NotImplementedException();
        }

        public Database SaveAndBuild(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}