using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterMemory.CodeGenerator.CodeAnalysis;

namespace MasterMemory.CodeGenerator.Generator
{
    public partial class CodeTemplate
    {
        public string Namespace { get; set; }
        public bool UnuseUnityAttribute { get; set; }
        public EnumDefinitions[] enumDefinitions { get; set; }
        public ElementDefinitions[] elementDefinitions { get; set; }
        public KeyTupleDefinitions[] keyTupleDefinitions { get; set; }
    }
}
