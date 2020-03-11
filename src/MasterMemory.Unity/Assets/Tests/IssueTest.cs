using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MasterMemory.Tests
{
    public class IssueTest
    {
        [Fact]
        public void Issue49()
        {
            var builder = new DatabaseBuilder().Append(new[]
            {
                new PersonModel { FirstName = "realname", LastName="reallast" },
                new PersonModel { FirstName = "fakefirst", LastName="fakelast" },
            });

            var data = builder.Build();
            var database = new MemoryDatabase(data);
            
            var entries = database.PersonModelTable.FindClosestByFirstNameAndLastName(("real", "real"), false);
            var firstEntry = entries.FirstOrDefault();

            var firstIs = firstEntry.FirstName;
            
        }
    }
}
