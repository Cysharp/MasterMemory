using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory.CodeGenerator.CodeAnalysis
{
    public class EnumDefinitions
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string FullName { get; set; }
        public string UnderlyingType { get; set; }
    }

    public class ElementDefinitions
    {
        public string FullName { get; set; }
    }

    public class KeyTupleDefinitions : IEquatable<KeyTupleDefinitions>
    {
        public string[] FullNames { get; set; }

        public bool Equals(KeyTupleDefinitions other)
        {
            return this.FullNames.SequenceEqual(other.FullNames);
        }
    }
}
