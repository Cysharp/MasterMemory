using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPerfLiteDB;

namespace TestPerfLiteDB
{
    public class Dictionary_Test : ITest
    {
        private string _filename;
        private int _count;

        Dictionary<int, TestDoc> dict;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public Dictionary_Test(int count)
        {
            _count = count;
            _filename = "dict-" + Guid.NewGuid().ToString("n") + ".db";
            dict = new Dictionary<int, TestPerfLiteDB.TestDoc>();
        }

        public void Insert()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                var v = new TestDoc
                {
                    id = doc["_id"].AsInt32,
                    name = doc["name"].AsString,
                    lorem = doc["lorem"].AsString
                };

                dict.Add(v.id, v);
            }
        }

        public void Bulk()
        {

        }

        public void CreateIndex()
        {

        }

        public void Dispose()
        {

        }

        public void Prepare()
        {

        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                TestDoc d;
                dict.TryGetValue(i, out d);
            }
        }

        public void Update()
        {

        }
    }

    public class ConcurrentDictionary_Test : ITest
    {
        private string _filename;
        private int _count;

        ConcurrentDictionary<int, TestDoc> dict;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public ConcurrentDictionary_Test(int count)
        {
            _count = count;
            _filename = "concurrentdict-" + Guid.NewGuid().ToString("n") + ".db";
            dict = new ConcurrentDictionary<int, TestPerfLiteDB.TestDoc>();
        }

        public void Insert()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                var v = new TestDoc
                {
                    id = doc["_id"].AsInt32,
                    name = doc["name"].AsString,
                    lorem = doc["lorem"].AsString
                };

                dict.TryAdd(v.id, v);
            }
        }

        public void Bulk()
        {

        }

        public void CreateIndex()
        {

        }

        public void Dispose()
        {

        }

        public void Prepare()
        {

        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                TestDoc d;
                dict.TryGetValue(i, out d);
            }
        }

        public void Update()
        {

        }
    }

    public class ImmutableDictionary_Test : ITest
    {
        private string _filename;
        private int _count;

        ImmutableDictionary<int, TestDoc> dict;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public ImmutableDictionary_Test(int count)
        {
            _count = count;
            _filename = "immutabledict-" + Guid.NewGuid().ToString("n") + ".db";
            //dict = new ImmutableDictionary<int, TestPerfLiteDB.TestDoc>();
        }

        public void Insert()
        {
            var builder = ImmutableDictionary.CreateBuilder<int, TestDoc>();
            foreach (var doc in Helper.GetDocs(_count))
            {
                var v = new TestDoc
                {
                    id = doc["_id"].AsInt32,
                    name = doc["name"].AsString,
                    lorem = doc["lorem"].AsString
                };

                builder.Add(v.id, v);
            }

            dict = builder.ToImmutableDictionary();
        }

        public void Bulk()
        {

        }

        public void CreateIndex()
        {

        }

        public void Dispose()
        {

        }

        public void Prepare()
        {

        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                TestDoc d;
                dict.TryGetValue(i, out d);
            }
        }

        public void Update()
        {

        }
    }
}
