using System.IO;

namespace MasterMemory.Tests
{
    // tests/MasterMemory.Tests.Aot is an application that is published with Native AOT and runs checks over every part
    // of the library. The same checks run here on the JIT: they have to pass here first, and one that passes here and
    // fails in the native executable is something Native AOT broke.
    public class AotChecksTest
    {
        readonly ITestOutputHelper output;

        public AotChecksTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TheChecksOfTheNativeAotApplicationPassOnTheJit()
        {
            var writer = new StringWriter();
            var failed = MasterMemory.AotTests.AotChecks.Run(writer);
            output.WriteLine(writer.ToString());

            failed.ShouldBe(0, writer.ToString());
            writer.ToString().ShouldContain("16 passed, 0 failed");
        }
    }
}
