using MessagePack;
using MessagePack.Resolvers;

namespace MasterMemory.Tests;

[CompositeResolver(typeof(MasterMemoryResolver), typeof(StandardResolver))]
public partial class MessagePackResolver;