#pragma warning disable
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

            sampleTable.TableName.ShouldBe("s_a_m_p_l_e");

            sampleTable.Properties[0].Name.ShouldBe("Id");
            sampleTable.Properties[0].NameLowerCamel.ShouldBe("id");
            sampleTable.Properties[0].NameSnakeCase.ShouldBe("id");

            sampleTable.Properties[2].Name.ShouldBe("FirstName");
            sampleTable.Properties[2].NameLowerCamel.ShouldBe("firstName");
            sampleTable.Properties[2].NameSnakeCase.ShouldBe("first_name");

            var primary = sampleTable.Indexes[0];
            primary.IsUnique.ShouldBeTrue();
            primary.IndexProperties[0].Name.ShouldBe("Id");
        }
    }
}
