namespace MasterMemory.SourceGenerator.Tests;

public abstract class TestBase(ITestOutputHelper testoutputHelper)
{
    protected CodeGeneratorHelper Helper = new CodeGeneratorHelper(testoutputHelper, "MAM");

    protected void WriteLine(string message)
    {
        testoutputHelper.WriteLine(message);
    }
}
