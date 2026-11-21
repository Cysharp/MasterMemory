using MasterMemory;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyProj;

[assembly: MasterMemoryGeneratorOptions(Namespace = "MyProj")]

// If you want to use init, copy-and-paste this.
namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit { }
}



public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum Gender
{
    Male, Female, Unknown
}

// table definition marked by MemoryTableAttribute.
// database-table must be serializable by MessagePack-CSsharp
[MemoryTable("person"), MessagePackObject(true)]
public record Person
{
    // index definition by attributes.
    [PrimaryKey]
    public int PersonId { get; init; }

    // secondary index can add multiple(discriminated by index-number).
    [SecondaryKey(0), NonUnique]
    [SecondaryKey(1, keyOrder: 1), NonUnique]
    public int Age { get; init; }

    [SecondaryKey(2), NonUnique]
    [SecondaryKey(1, keyOrder: 0), NonUnique]
    public Gender Gender { get; init; }

    public string Name { get; init; }
}

// MessagePack for C# v4 needs no resolver registration: the generated MemoryDatabase / DatabaseBuilder
// carry the formatters of every table and default to MessagePackFormatterFactory.DefaultAot, which is IL2CPP safe.
