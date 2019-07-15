using MasterMemory;

namespace TestPerfLiteDB
{
    public sealed class DatabaseBuilder : DatabaseBuilderBase
    {
        public DatabaseBuilder() : this(null) { }
        public DatabaseBuilder(MessagePack.IFormatterResolver resolver) : base(resolver) { }


        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<TestDoc> dataSource)
        {
            AppendCore(dataSource, x => x.id, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

    }
}