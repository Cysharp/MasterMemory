using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MasterMemory.Tests
{
    public class MetaTest
    {
        [Fact]
        public void Meta()
        {
            var metaDb = MemoryDatabase.GetMetaDatabase();

            var sampleTable = metaDb.GetTableInfo("s_a_m_p_l_e");

            sampleTable.TableName.Should().Be("s_a_m_p_l_e");

            sampleTable.Properties[0].Name.Should().Be("Id");
            sampleTable.Properties[0].NameLowerCamel.Should().Be("id");
            sampleTable.Properties[0].NameSnakeCase.Should().Be("id");

            sampleTable.Properties[2].Name.Should().Be("FirstName");
            sampleTable.Properties[2].NameLowerCamel.Should().Be("firstName");
            sampleTable.Properties[2].NameSnakeCase.Should().Be("first_name");

            var primary = sampleTable.Indexes[0];
            primary.IsUnique.Should().BeTrue();
            primary.IndexProperties[0].Name.Should().Be("Id");
        }
    }
}
