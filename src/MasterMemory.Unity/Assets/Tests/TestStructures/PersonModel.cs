using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Tests.TestStructures
{

    [MemoryTable("people"), MessagePackObject(true)]
    public class PersonModel
    {
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(1, keyOrder: 1), NonUnique]
        public string LastName { get; set; }

        [SecondaryKey(2), NonUnique]
        [SecondaryKey(1, keyOrder: 0), NonUnique]
        public string FirstName { get; set; }

        [PrimaryKey] public string RandomId { get; set; }
    }
}
