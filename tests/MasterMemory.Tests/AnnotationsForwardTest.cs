using System;

namespace MasterMemory.Tests
{
    // The attributes were in MasterMemory.Annotations.dll until v3. An assembly compiled against that one names them there,
    // and the MasterMemory.Annotations of v4 forwards each of them to MasterMemory.dll.
    public class AnnotationsForwardTest
    {
        [Theory]
        [InlineData("MasterMemory.MemoryTableAttribute", typeof(MemoryTableAttribute))]
        [InlineData("MasterMemory.PrimaryKeyAttribute", typeof(PrimaryKeyAttribute))]
        [InlineData("MasterMemory.SecondaryKeyAttribute", typeof(SecondaryKeyAttribute))]
        [InlineData("MasterMemory.NonUniqueAttribute", typeof(NonUniqueAttribute))]
        [InlineData("MasterMemory.StringComparisonOptionAttribute", typeof(StringComparisonOptionAttribute))]
        public void NameInAnnotationsAssemblyIsTheTypeInMasterMemory(string typeName, Type expected)
        {
            var type = Type.GetType(typeName + ", MasterMemory.Annotations", throwOnError: true);

            type.ShouldBe(expected);
            type.Assembly.GetName().Name.ShouldBe("MasterMemory");
        }

        [Fact]
        public void AnnotationsAssemblyHasNoTypeOfItsOwn()
        {
            var annotations = System.Reflection.Assembly.Load("MasterMemory.Annotations");

            annotations.GetTypes().ShouldBeEmpty();
            annotations.GetForwardedTypes().Length.ShouldBe(5);
        }
    }
}
