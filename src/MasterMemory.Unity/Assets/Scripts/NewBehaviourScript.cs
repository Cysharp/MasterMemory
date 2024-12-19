using MasterMemory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: MasterMemoryGeneratorOptions(Namespace = "MyProj")]

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var meta = MyProj.Tables.FooTable.CreateMetaTable();
        foreach (var item in meta.Indexes)
        {
            Debug.Log(item.IsPrimaryIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}


[MemoryTable("foo")]
public class Foo
{
    [PrimaryKey]
    public int MyProperty { get; set; }
}