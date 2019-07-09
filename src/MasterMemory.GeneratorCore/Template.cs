using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.GeneratorCore
{
    public partial class DatabaseBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class MemoryDatabaseTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class ImmutableBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class TableTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext GenerationContext { get; set; }
    }
}
