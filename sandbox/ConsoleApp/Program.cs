using MasterMemory;
using MasterMemory.Annotations;
using MessagePack;
using System;
using System.IO;

namespace ConsoleApp
{
    [MemoryTable("monster")]
    public class Monster
    {
        [PrimaryKey]
        public int MonsterId { get; set; }
        public string Name { get; set; }
        public int MaxHp { get; set; }
    }


    public enum Gender
    {
        Male, Female
    }

    [MemoryTable("person"), MessagePackObject(true)]
    public class Person
    {
        [PrimaryKey]
        public int PersonId { get; set; }
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(2, keyOrder: 1), NonUnique]
        public int Age { get; set; }
        [SecondaryKey(1), NonUnique]
        [SecondaryKey(2, keyOrder: 0), NonUnique]
        public Gender Gender { get; set; }
        public string Name { get; set; }
    }



    class Program
    {
        static void Main(string[] args)
        {
            var bin = new DatabaseBuilder().Append(new[]
            {
                new Monster { MonsterId = 1, Name = "Foo", MaxHp = 100 }
            }).Build();




            var db = new MemoryDatabase(File.ReadAllBytes("db.bin"));

            // .PersonTable.FindByPersonIdもコード生成により型が付いてる
            Person person = db.PersonTable.FindByPersonId(10);

            // 女性の23歳を取得。戻り値は複数。
            RangeView<Person> result = db.PersonTable.FindByGenderAndAge((Gender.Female, 23));


            // 31歳に最も近い人を取得
            RangeView<Person> age1 = db.PersonTable.FindClosestByAge(31);


            // 20歳から29際の人を取得
            RangeView<Person> age2 = db.PersonTable.FindRangeByAge(20, 29);



        }
    }
}
