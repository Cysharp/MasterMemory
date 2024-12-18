using Xunit.Abstractions;

namespace MasterMemory.SourceGenerator.Tests
{
    public class IncrementalGeneratorTest
    {
        void VerifySourceOutputReasonIsCached((string Key, string Reasons)[] reasons)
        {
            var reason = reasons.FirstOrDefault(x => x.Key == "SourceOutput").Reasons;
            reason.Should().Be("Cached");
        }

        void VerifySourceOutputReasonIsNotCached((string Key, string Reasons)[] reasons)
        {
            var reason = reasons.FirstOrDefault(x => x.Key == "SourceOutput").Reasons;
            reason.Should().NotBe("Cached");
        }

        [Fact]
        public void CheckReasons()
        {
            var step1 = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

            var step2 = """
[MemoryTable("item")]
public class Item
{
    // add unrelated line
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

            var step3 = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId2 { get; set; } // changed name
}
""";

            var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("MasterMemory.SyntaxProvider.", step1, step2, step3);

            VerifySourceOutputReasonIsCached(reasons[1]);
            VerifySourceOutputReasonIsNotCached(reasons[2]);
        }
    }
}
