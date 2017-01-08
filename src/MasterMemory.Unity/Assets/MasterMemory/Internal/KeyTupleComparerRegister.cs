using System;
using System.Reflection;
using System.Linq;
using ZeroFormatter;

namespace MasterMemory.Internal
{
    internal static class KeyTupleComparerRegister
    {
        static readonly MethodInfo[] registers; // 2 = 0, 3 = 1,...
        static readonly object[] emptyArgs = new object[0];

        static KeyTupleComparerRegister()
        {
            registers =
#if !UNITY_5
            typeof(KeyTupleComparer).GetTypeInfo().GetMethods()
#else
            typeof(KeyTupleComparer).GetMethods()
#endif
                .Where(x => x.Name == "Register")
                .OrderBy(x => x.GetGenericArguments().Length)
                .ToArray();
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
            if (typeof(TKey).GetInterfaces().Contains(typeof(IKeyTuple)))
            {
                var args = typeof(TKey).GetGenericArguments();
                registers[args.Length - 2].MakeGenericMethod(args).Invoke(null, emptyArgs);
            }
#endif
        }
    }
}