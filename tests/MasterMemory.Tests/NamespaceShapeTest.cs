using MasterMemory.Tests.FileScoped;
using MasterMemory.Tests.TestStructures;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // Tables whose declaration the generated code can only reach by its full name. That the project compiles is most of
    // the test; the rest is that the tables work.
    public class NamespaceShapeTest
    {
        [Fact]
        public void FileScopedNamespaceAndConditionalUsings()
        {
            var bin = new DatabaseBuilder()
                .Append(new[]
                {
                    new FileScopedMaster { Id = 2, Element = Element.Water, Code = "b" },
                    new FileScopedMaster { Id = 1, Element = Element.Fire, Code = "a" },
                    new FileScopedMaster { Id = 3, Element = Element.Water, Code = "c" },
                })
                .Build();

            var table = new MemoryDatabase(bin).FileScopedMasterTable;

            table.FindById(3).Code.ShouldBe("c");
            table.FindByCode("a").Id.ShouldBe(1);
            table.FindByElement(Element.Water).AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(new[] { 2, 3 });
            table.FindByElement(Element.Wind).Count.ShouldBe(0);
        }

        [Fact]
        public void NestedType()
        {
            var bin = new DatabaseBuilder()
                .Append(new[]
                {
                    new NestedOwner.NestedMaster { Id = 10, Name = "ten" },
                    new NestedOwner.NestedMaster { Id = 5, Name = "five" },
                })
                .Build();

            var db = new MemoryDatabase(bin);
            db.NestedMasterTable.All.AsReadOnlyList().Select(x => x.Name).ToArray().ShouldBe(new[] { "five", "ten" });

            // through the methods that change a table as well: they name the row type in their signatures
            var builder = db.ToDatabaseBuilder();
            builder.AddOrReplace(new[] { new NestedOwner.NestedMaster { Id = 7, Name = "seven" } });
            builder.RemoveNestedMaster(5);
            builder.ToMemoryDatabase().NestedMasterTable.All.AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(new[] { 7, 10 });
        }
    }
}
