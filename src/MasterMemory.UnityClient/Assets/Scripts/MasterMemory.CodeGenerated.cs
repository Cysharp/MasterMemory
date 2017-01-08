#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MasterMemory
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::ZeroFormatter.Formatters;

    public static partial class MasterMemoryInitializer
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            MasterMemory.MasterMemoryComparer<TesTes.MyDummyEnum1>.Default = new MasterMemory.Comparers.TesTes_MyDummyEnum1_Comparer();
            MasterMemory.MasterMemoryComparer<MyDummyEnum2>.Default = new MasterMemory.Comparers.MyDummyEnum2_Comparer();
            MasterMemory.MasterMemoryComparer<MyDummyEnum3>.Default = new MasterMemory.Comparers.MyDummyEnum3_Comparer();
            ZeroFormatter.Formatters.Formatter.RegisterList<DefaultResolver, MasterMemory.Tests.Sample>();
            MasterMemory.KeyTupleComparer.Register<string, string>();
            MasterMemory.KeyTupleComparer.Register<string, string, int>();
            MasterMemory.KeyTupleComparer.Register<int, int, string, string>();
            MasterMemory.KeyTupleComparer.Register<string, int>();
            MasterMemory.KeyTupleComparer.Register<MyDummyEnum2, MyDummyEnum3>();
        }
    }
}

namespace MasterMemory.Comparers
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::ZeroFormatter.Formatters;


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
