using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class MasterMemoryHintAttribute : Attribute
    {
        public MasterMemoryHintAttribute(Type elementType, params Type[] keyTypes)
        {

        }
    }
}