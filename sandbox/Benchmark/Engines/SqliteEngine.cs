using System;
using System.Data;
using System.Data.SQLite;

namespace Benchmark.Engines
{
    /// <summary>SQLite through System.Data.SQLite, a row being three columns.</summary>
    public sealed class SqliteEngine : IDisposable
    {
        const string Select = "SELECT id, name, lorem FROM col WHERE id = @id";

        readonly SQLiteConnection connection;
        readonly SQLiteCommand prepared;
        readonly SQLiteParameter preparedId;

        SqliteEngine(SQLiteConnection connection)
        {
            this.connection = connection;
            prepared = new SQLiteCommand(Select, connection);
            preparedId = prepared.Parameters.Add("id", DbType.Int32);
            prepared.Prepare();
        }

        public static void Build(string path, TestDoc[] rows)
        {
            using var connection = Open("Data Source=" + path);
            Fill(connection, rows);
        }

        // immutable: false is how a file is opened when nothing is said, and then every query locks and unlocks the file,
        // which costs far more than the query itself. immutable: true (the URI parameter immutable=1) tells SQLite that
        // nobody writes the file while it is open: no locks, the fastest way it has to read a file.
        public static SqliteEngine OpenFile(string path, bool immutable)
        {
            if (!immutable) return new SqliteEngine(Open("Data Source=" + path));

            var uri = new Uri(path).AbsoluteUri + "?immutable=1";
            return new SqliteEngine(Open("FullUri=" + uri + ";Read Only=True"));
        }

        public static SqliteEngine CreateInMemory(TestDoc[] rows)
        {
            var connection = Open("Data Source=:memory:");
            Fill(connection, rows);
            return new SqliteEngine(connection);
        }

        /// <summary>The statement is prepared once and reused: what a program that cares for speed does.</summary>
        public TestDoc Find(int id)
        {
            preparedId.Value = id;
            using var reader = prepared.ExecuteReader();
            return Read(reader, id);
        }

        /// <summary>A command is created and parsed for every query: the plain way to write it.</summary>
        public TestDoc FindNaive(int id)
        {
            using var command = new SQLiteCommand(Select, connection);
            command.Parameters.Add("id", DbType.Int32).Value = id;
            using var reader = command.ExecuteReader();
            return Read(reader, id);
        }

        public void Dispose()
        {
            prepared.Dispose();
            connection.Dispose();
        }

        static SQLiteConnection Open(string connectionString)
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        static void Fill(SQLiteConnection connection, TestDoc[] rows)
        {
            using (var create = new SQLiteCommand("CREATE TABLE col (id INTEGER NOT NULL PRIMARY KEY, name TEXT, lorem TEXT)", connection))
            {
                create.ExecuteNonQuery();
            }

            using var transaction = connection.BeginTransaction();
            using var insert = new SQLiteCommand("INSERT INTO col (id, name, lorem) VALUES (@id, @name, @lorem)", connection, transaction);
            var id = insert.Parameters.Add("id", DbType.Int32);
            var name = insert.Parameters.Add("name", DbType.String);
            var lorem = insert.Parameters.Add("lorem", DbType.String);
            foreach (var row in rows)
            {
                id.Value = row.Id;
                name.Value = row.Name;
                lorem.Value = row.Lorem;
                insert.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        static TestDoc Read(SQLiteDataReader reader, int id)
        {
            if (!reader.Read()) throw new InvalidOperationException("SQLite has no row of id " + id);
            return new TestDoc { Id = reader.GetInt32(0), Name = reader.GetString(1), Lorem = reader.GetString(2) };
        }
    }
}
