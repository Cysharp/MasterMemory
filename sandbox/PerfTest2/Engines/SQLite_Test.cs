using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPerfLiteDB
{
    public class SQLite_Test : ITest
    {
        private string _filename;
        private SQLiteConnection _db;
        private int _count;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public SQLite_Test(int count, string password, bool journal, bool memory = false)
        {
            _count = count;
            _filename = "sqlite-" + Guid.NewGuid().ToString("n") + ".db";
            
            if (memory)
            {
                var cs = "Data Source=:memory:;New=True;";
                _db = new SQLiteConnection(cs);
            }
            else
            {
                var cs = "Data Source=" + _filename;
                if (password != null) cs += "; Password=" + password;
                if (journal == false) cs += "; Journal Mode=Off";
                _db = new SQLiteConnection(cs);
            }
        }

        public void Prepare()
        {
            _db.Open();

            var table = new SQLiteCommand("CREATE TABLE col (id INTEGER NOT NULL PRIMARY KEY, name TEXT, lorem TEXT)", _db);
            table.ExecuteNonQuery();

            var table2 = new SQLiteCommand("CREATE TABLE col_bulk (id INTEGER NOT NULL PRIMARY KEY, name TEXT, lorem TEXT)", _db);
            table2.ExecuteNonQuery();
        }

        public void Insert()
        {
            // standard insert is slow, mod same as Bulk
            using (var trans = _db.BeginTransaction())
            {
                var cmd = new SQLiteCommand("INSERT INTO col (id, name, lorem) VALUES (@id, @name, @lorem)", _db);

                cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
                cmd.Parameters.Add(new SQLiteParameter("name", DbType.String));
                cmd.Parameters.Add(new SQLiteParameter("lorem", DbType.String));

                foreach (var doc in Helper.GetDocs(_count))
                {
                    cmd.Parameters["id"].Value = doc["_id"].AsInt32;
                    cmd.Parameters["name"].Value = doc["name"].AsString;
                    cmd.Parameters["lorem"].Value = doc["lorem"].AsString;

                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
        }

        public void Bulk()
        {
            using (var trans = _db.BeginTransaction())
            {
                var cmd = new SQLiteCommand("INSERT INTO col_bulk (id, name, lorem) VALUES (@id, @name, @lorem)", _db);

                cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
                cmd.Parameters.Add(new SQLiteParameter("name", DbType.String));
                cmd.Parameters.Add(new SQLiteParameter("lorem", DbType.String));

                foreach (var doc in Helper.GetDocs(_count))
                {
                    cmd.Parameters["id"].Value = doc["_id"].AsInt32;
                    cmd.Parameters["name"].Value = doc["name"].AsString;
                    cmd.Parameters["lorem"].Value = doc["lorem"].AsString;

                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
        }

        public void Update()
        {
            var cmd = new SQLiteCommand("UPDATE col SET name = @name, lorem = @lorem WHERE id = @id", _db);

            cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("name", DbType.String));
            cmd.Parameters.Add(new SQLiteParameter("lorem", DbType.String));

            foreach (var doc in Helper.GetDocs(_count))
            {
                cmd.Parameters["id"].Value = doc["_id"].AsInt32;
                cmd.Parameters["name"].Value = doc["name"].AsString;
                cmd.Parameters["lorem"].Value = doc["lorem"].AsString;

                cmd.ExecuteNonQuery();
            }
        }

        public void CreateIndex()
        {
            var cmd = new SQLiteCommand("CREATE INDEX idx1 ON col (name)", _db);

            cmd.ExecuteNonQuery();
        }

        public void Query()
        {
            var cmd = new SQLiteCommand("SELECT * FROM col WHERE id = @id", _db);

            cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));

            for (var i = 0; i < _count; i++)
            {
                cmd.Parameters["id"].Value = i;

                var r = cmd.ExecuteReader();

                r.Read();

                var name = r.GetString(1);
                var lorem = r.GetString(2);

                r.Close();
            }
        }

        public void Delete()
        {
            var cmd = new SQLiteCommand("DELETE FROM col", _db);

            cmd.ExecuteNonQuery();
        }

        public void Drop()
        {
            var cmd = new SQLiteCommand("DROP TABLE col_bulk", _db);

            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
