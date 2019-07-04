using System;
using Xunit;

namespace MasterMemory.Tests2
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var builder = new DatabaseBuilder();
            builder.Append(new[]
            {
                new MyClass{ Id = 100, Timestamp = new DateTime(2012,12,12) },
                new MyClass{ Id = 101, Timestamp = new DateTime(2012,12,13) },
                new MyClass{ Id = 50, Timestamp = new DateTime(2012,12,14) },
                new MyClass{ Id = 70, Timestamp = new DateTime(2012,12,14) },
                new MyClass{ Id = 80, Timestamp = new DateTime(2012,12,15) },
            });

            var database = builder.Build();

            var db = new MemoryDatabase(database);

            var foo = db.MyClassTable.FindById(50);



            
        }
    }
}
