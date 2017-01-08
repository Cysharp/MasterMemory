using UnityEngine;
using System.Collections;
using RuntimeUnitTestToolkit;
using MasterMemory.Tests;
using ZeroFormatter.Formatters;
using MasterMemory;
using ZeroFormatter;
using TesTes;
using System.Collections.Generic;
using System;

[assembly: MasterMemoryHintAttribute(typeof(Sample))] // dummy
[assembly: MasterMemoryHintAttribute(typeof(Sample), typeof(int), typeof(KeyTuple<string, string>), typeof(KeyTuple<string, string, int>), typeof(KeyTuple<int,int, string, string>), typeof(KeyTuple<string,int>))]
[assembly: MasterMemoryHintAttribute(typeof(Sample), typeof(MyDummyEnum1), typeof(KeyTuple<MyDummyEnum2, MyDummyEnum3>))]

public class TesTes_MyDummyEnum1_Comparer : IComparer<TesTes.MyDummyEnum1>
{
    public int Compare(MyDummyEnum1 x, MyDummyEnum1 y)
    {
        return ((byte)x).CompareTo((byte)y);
    }
}

namespace TesTes
{
    public enum MyDummyEnum1 : byte
    {
        A, B, C
    }
}

public enum MyDummyEnum2 : int
{
    A, B, C
}

public enum MyDummyEnum3 : uint
{
    A, B, C
}

public class Loader
{
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Register()
    {
        UnitTest.RegisterAllMethods<BinarySearchTest>();
        UnitTest.RegisterAllMethods<DatabaseTest>();
        UnitTest.RegisterAllMethods<KeyTupleMemoryTest>();
        UnitTest.RegisterAllMethods<MemoryTest>();
        UnitTest.RegisterAllMethods<RangeViewTest>();

        ZeroFormatter.ZeroFormatterInitializer.Register();
        ZeroFormatter.Formatters.Formatter.RegisterList<DefaultResolver, Sample>();
    }
}
