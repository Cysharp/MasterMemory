//using Raven.Client;
//using Raven.Client.Embedded;
//using Raven.Client.Indexes;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TestPerfLiteDB
//{
//    public class TestDocCreation : AbstractIndexCreationTask<TestDoc>
//    {
//        public TestDocCreation()
//        {
//            Map = xs => xs.Select(x => new { x.id });
//        }
//    }

//    public class RavenDB_Test : ITest
//    {
//        private string _filename;
//        private int _count;
//        private bool isinmemory;

//        IDocumentStore store;
//        public int Count { get { return _count; } }
//        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

//        public RavenDB_Test(int count, bool isinmemory)
//        {
//            _count = count;
//            _filename = "ravendb-" + Guid.NewGuid().ToString("n") + ".db";
//            this.isinmemory = isinmemory;
//        }

//        public void Bulk()
//        {
//            if (isinmemory)
//            {


//            }

//            //using (var store = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
//            //{
//            //    using (var bulk = store.BulkInsert())
//            //    {
//            //        bulk.Store(new MyClass { Id = 9999, MyProperty = 1000 });
//            //    }

//            //    var session = store.OpenSession();

//            //    store.ExecuteIndex(new MyClassIndex());

//            //    var huga = session.Load<MyClass>("MyClasses/9999");
//            //}
//        }

//        public void CreateIndex()
//        {
//            store.ExecuteIndex(new TestDocCreation());
//        }

//        public void Dispose()
//        {
//            store.Dispose();
//        }
//        IEnumerable<TestDoc> MakeDoc()
//        {
//            foreach (var doc in Helper.GetDocs(_count))
//            {
//                var v = new TestDoc
//                {
//                    id = doc["_id"].AsInt32,
//                    name = doc["name"].AsString,
//                    lorem = doc["lorem"].AsString
//                };

//                yield return v;
//            }
//        }


//        public void Insert()
//        {

//            using (var bulk = store.BulkInsert())
//            {
//                foreach (var item in MakeDoc())
//                {
//                    bulk.Store(item);
//                }
//            }

//        }

//        public void Prepare()
//        {
//            if (isinmemory)
//            {
//                store = new EmbeddableDocumentStore { RunInMemory = true }.Initialize();
//            }
//            else
//            {
//                // store = new EmbeddableDocumentStore { RunInMemory = true }.Initialize();
//            }
//        }

//        public void Query()
//        {
//            for (var i = 0; i < _count; i++)
//            {
//                using (var session = store.OpenSession())
//                {
//                    session.Load<TestDoc>("TestDoc/" + i);
//                }
//            }
//        }

//        public void Update()
//        {

//        }
//    }
//}
