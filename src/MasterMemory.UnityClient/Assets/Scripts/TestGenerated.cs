#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MasterMemory
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::MessagePack;

    public static partial class MasterMemoryInitializer
    {
        static bool registered = false;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            if(registered) return;
            registered = true;

            MasterMemory.MasterMemoryComparer<TesTes.MyDummyEnum1>.Default = new MasterMemory.Comparers.TesTes_MyDummyEnum1_Comparer();
            MasterMemory.MasterMemoryComparer<MyDummyEnum2>.Default = new MasterMemory.Comparers.MyDummyEnum2_Comparer();
            MasterMemory.MasterMemoryComparer<MyDummyEnum3>.Default = new MasterMemory.Comparers.MyDummyEnum3_Comparer();
            MasterMemory.MemoryKeyComparer.Register<string, string>();
            MasterMemory.MemoryKeyComparer.Register<string, string, int>();
            MasterMemory.MemoryKeyComparer.Register<int, int, string, string>();
            MasterMemory.MemoryKeyComparer.Register<string, int>();
            MasterMemory.MemoryKeyComparer.Register<MyDummyEnum2, MyDummyEnum3>();
        }
    }

	public class MasterMemoryResolver : global::MessagePack.IFormatterResolver
    {
        public static global::MessagePack.IFormatterResolver Instance = new MasterMemoryResolver();

        MasterMemoryResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = MasterMemoryResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class MasterMemoryResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static MasterMemoryResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(1)
            {
                {typeof(MasterMemory.Tests.Sample[]), 0 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ArrayFormatter<MasterMemory.Tests.Sample>();
                default: return null;
            }
        }
    }
}

namespace MasterMemory.Comparers
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;


    public class TesTes_MyDummyEnum1_Comparer : IComparer<TesTes.MyDummyEnum1>
    {
        public int Compare(TesTes.MyDummyEnum1 x, TesTes.MyDummyEnum1 y)
        {
            return ((Byte)x).CompareTo((Byte)y);
        }
    }


    public class MyDummyEnum2_Comparer : IComparer<MyDummyEnum2>
    {
        public int Compare(MyDummyEnum2 x, MyDummyEnum2 y)
        {
            return ((Int32)x).CompareTo((Int32)y);
        }
    }


    public class MyDummyEnum3_Comparer : IComparer<MyDummyEnum3>
    {
        public int Compare(MyDummyEnum3 x, MyDummyEnum3 y)
        {
            return ((UInt32)x).CompareTo((UInt32)y);
        }
    }

}

#pragma warning restore 618
#pragma warning restore 612
#pragma warning restore 414
#pragma warning restore 168
