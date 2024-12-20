#nullable disable

using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.GeneratorCore
{
    public partial class DatabaseBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }

        public string ClassName => PrefixClassName + "DatabaseBuilder";
    }

    public partial class MemoryDatabaseTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "MemoryDatabase";
    }

    public partial class MetaMemoryDatabaseTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "MetaMemoryDatabase";
    }

    public partial class ImmutableBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "ImmutableBuilder";
    }

    public partial class MessagePackResolverTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "MasterMemoryResolver";
    }

    public partial class TableTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext GenerationContext { get; set; }

        public bool ThrowKeyIfNotFound { get; set; }
    }
}
