using System;
using System.Reflection;
using System.Linq;

namespace MasterMemory.Internal
{
    internal static class MemoryKeyComparerRegister
    {
#if !UNITY_5
        static readonly MethodInfo[] registers; // 2 = 0, 3 = 1,...
        static readonly object[] emptyArgs = new object[0];
#endif

        static MemoryKeyComparerRegister()
        {
#if !UNITY_5
            registers =

            typeof(MemoryKeyComparer).GetRuntimeMethods()
                .Where(x => x.Name == "Register")
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();
#else
            // registers = null;
#endif
        }

        public static void RegisterDynamic<TKey>()
        {
#if !UNITY_5
            if (typeof(TKey).GetTypeInfo().ImplementedInterfaces.Contains(typeof(IMemoryKey)))
            {
                var args = typeof(TKey).GetTypeInfo().GenericTypeArguments;
                registers[args.Length - 2].MakeGenericMethod(args).Invoke(null, emptyArgs);
            }
#else
            // currently do nothing...
#endif
        }
    }
}