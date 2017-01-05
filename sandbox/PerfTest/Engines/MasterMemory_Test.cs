using MasterMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPerfLiteDB
{
    public class MasterMemory_Test : ITest
    {
        private string _filename;
        private int _count;

        Memory<int, TestPerfLiteDB.TestDoc> memory;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public MasterMemory_Test(int count)
        {
            _count = count;
            _filename = "mastermemory-" + Guid.NewGuid().ToString("n") + ".db";

        }

        public IEnumerable<TestDoc> MakeDoc()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                var v = new TestDoc
                {
                    id = doc["_id"].AsInt32,
                    name = doc["name"].AsString,
                    lorem = doc["lorem"].AsString
                };

                yield return v;
            }
        }

        public void Insert()
        {
            memory = new Memory<int, TestPerfLiteDB.TestDoc>(MakeDoc(), x => x.id);
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
                //TestDoc d;
                memory.FindOrDefault(i);
            }
        }

        public void Update()
        {

        }
    }
}
