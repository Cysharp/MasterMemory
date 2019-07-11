using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TestPerfLiteDB
{
    public class LiteDB_Test : ITest
    {
        private string _filename;
        private LiteEngine _db;
        private int _count;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public LiteDB_Test(int count, string password, LiteDB.FileOptions options)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var disk = new FileDiskService(_filename, options);

            _db = new LiteEngine(disk, password);
        }

        public LiteDB_Test(int count)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var ms = new MemoryStream();
            var disk = new LiteDB.StreamDiskService(ms);
            //var disk = new FileDiskService(_filename, options);

            _db = new LiteEngine(disk);
        }

        public void Prepare()
        {
        }

        public void Insert()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                _db.Insert("col", doc);
            }
        }

        public void Bulk()
        {
            _db.Insert("col_bulk", Helper.GetDocs(_count));
        }

        public void Update()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                _db.Update("col", doc);
            }
        }

        public void CreateIndex()
        {
            _db.EnsureIndex("col", "name", false);
        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                _db.Find("col", LiteDB.Query.EQ("_id", i)).Single();
            }
        }

        public void Delete()
        {
            _db.Delete("col", LiteDB.Query.All());
        }

        public void Drop()
        {
            _db.DropCollection("col_bulk");
        }

        public void Dispose()
        {
            _db.Dispose();
            File.Delete(_filename);
        }
    }
}
