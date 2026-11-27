using System.Runtime.CompilerServices;
using MasterMemory;

// The attributes live in MasterMemory.dll since v4. An assembly compiled against MasterMemory.Annotations of v3
// names them in this assembly, and these forwards send it to where they are now.
[assembly: TypeForwardedTo(typeof(MemoryTableAttribute))]
[assembly: TypeForwardedTo(typeof(PrimaryKeyAttribute))]
[assembly: TypeForwardedTo(typeof(SecondaryKeyAttribute))]
[assembly: TypeForwardedTo(typeof(NonUniqueAttribute))]
[assembly: TypeForwardedTo(typeof(StringComparisonOptionAttribute))]
