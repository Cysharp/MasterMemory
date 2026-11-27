// The using directives sit inside #if (issue 109) and the namespace is file-scoped: two shapes the generator used to
// copy its using directives from, and got wrong.
#if !MASTERMEMORY_TESTS_WITHOUT_TABLES
using MasterMemory;
using MessagePack;
#endif

namespace MasterMemory.Tests.FileScoped;

public enum Element
{
    Fire, Water, Wind
}

[MemoryTable("file_scoped"), MessagePackObject(true)]
public class FileScopedMaster
{
    [PrimaryKey]
    public int Id { get; set; }

    // a key whose type is declared in this namespace only
    [SecondaryKey(0), NonUnique]
    public Element Element { get; set; }

    [SecondaryKey(1)]
    public string Code { get; set; }
}
