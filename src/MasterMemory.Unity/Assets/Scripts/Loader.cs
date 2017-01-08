using UnityEngine;
using System.Collections;
using RuntimeUnitTestToolkit;
using MasterMemory.Tests;
using ZeroFormatter.Formatters;

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
