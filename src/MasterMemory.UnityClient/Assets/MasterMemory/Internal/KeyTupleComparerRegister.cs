using System;
using System.Reflection;
using System.Linq;
using ZeroFormatter;

namespace MasterMemory.Internal
{
    internal static class KeyTupleComparerRegister
    {
        // static readonly MethodInfo[] registers; // 2 = 0, 3 = 1,...
        // static readonly object[] emptyArgs = new object[0];

        static KeyTupleComparerRegister()
        {
#if !UNITY_5
            registers =

            typeof(KeyTupleComparer).GetTypeInfo().GetMethods()
                .Where(x => x.Name == "Register")
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();
#else
            
#endif
        }

        public static void RegisterDynamic<TKey>()
        {
#if !UNITY_5
            if (typeof(TKey).GetTypeInfo().GetInterfaces().Contains(typeof(IKeyTuple)))
            {
                var args = typeof(TKey).GetTypeInfo().GetGenericArguments();
                registers[args.Length - 2].MakeGenericMethod(args).Invoke(null, emptyArgs);
            }
#else
            /*
            if (typeof(TKey).GetInterfaces().Contains(typeof(IKeyTuple)))
            {
                var args = typeof(TKey).GetGenericArguments();
                registers[args.Length - 2].MakeGenericMethod(args).Invoke(null, emptyArgs);
            }
            */
#endif
        }
    }
}