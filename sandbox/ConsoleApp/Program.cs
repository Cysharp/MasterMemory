using MasterMemory;
using System.Linq;
using MessagePack;
using System;
using System.IO;
using System.Buffers;
using System.Linq.Expressions;

namespace ConsoleApp
{
    [MemoryTable("monster"), MessagePackObject(true)]
    public class Monster
    {
        [PrimaryKey]
        public int MonsterId { get; }
        public string Name { get; }
        public int MaxHp { get; }

        public Monster(int MonsterId, string Name, int MaxHp)
        {
            this.MonsterId = MonsterId;
            this.Name = Name;
            this.MaxHp = MaxHp;
        }
    }




    public enum Gender
    {
        Male, Female
    }

    [MemoryTable("person"), MessagePackObject(true)]
    public class Person
    {
        [PrimaryKey(keyOrder: 1)]
        public int PersonId { get; set; }
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(2, keyOrder: 1), NonUnique]
        public int Age { get; set; }
        [SecondaryKey(1), NonUnique]
        [SecondaryKey(2, keyOrder: 0), NonUnique]
        public Gender Gender { get; set; }
        public string Name { get; set; }

        public Person()
        {
        }

        public Person(int PersonId, int Age, Gender Gender, string Name)
        {
            this.PersonId = PersonId;
            this.Age = Age;
            this.Gender = Gender;
            this.Name = Name;
        }

        public override string ToString()
        {
            return $"{PersonId} {Age} {Gender} {Name}";
        }
    }



    // IValidatableを実装すると検証対象になる
    [MemoryTable("quest"), MessagePackObject(true)]
    public class Quest : IValidatable<Quest>
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public int RewardId { get; set; }
        public int Cost { get; set; }

        void IValidatable<Quest>.Validate(IValidator<Quest> validator)
        {
            // relation貼ってる的なもののコレクションを取り出せる
            var items = validator.GetReferenceSet<Item>();
            // RewardIdが0以上のとき(0は報酬ナシのための特別なフラグという意味)
            if (this.RewardId > 0)
            {
                // Itemsのマスタに必ず含まれてなければ検証エラー（エラーが出ても続行はしてすべての検証結果を出す)
                items.Select(x => x.RewardId).Exists(() => this.Id);
            }

            // コストは10..20でなければ検証エラー
            validator.Validate(x => x.Cost >= 20);
            validator.Validate(x => x.Cost <= 20);

            // 一度しか呼ばれないゾーンなのでデータセット全体の検証をしたい時に使える。
            if (validator.CallOnce())
            {
                var quests = validator.GetTableSet();
                // シーケンシャルになってないとダメ
                quests.Sequential(x => x.Id);
            }
        }
    }

    [MemoryTable("item"), MessagePackObject(true)]
    public class Item
    {
        [PrimaryKey]
        public int RewardId { get; set; }
    }

    class ByteBufferWriter : IBufferWriter<byte>
    {
        byte[] buffer;
        int index;

        public int CurrentOffset => index;
        public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);
        public ReadOnlyMemory<byte> WrittenMemory => new ReadOnlyMemory<byte>(buffer, 0, index);

        public ByteBufferWriter()
        {
            buffer = new byte[1024];
            index = 0;
        }

        public void Advance(int count)
        {
            index += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            AGAIN:
            var nextSize = index + sizeHint;
            if (buffer.Length < nextSize)
            {
                Array.Resize(ref buffer, Math.Max(buffer.Length * 2, nextSize));
            }

            if (sizeHint == 0)
            {
                var result = new Memory<byte>(buffer, index, buffer.Length - index);
                if (result.Length == 0)
                {
                    sizeHint = 1024;
                    goto AGAIN;
                }
                return result;
            }
            else
            {
                return new Memory<byte>(buffer, index, sizeHint);
            }
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory(sizeHint).Span;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bin = new DatabaseBuilder().Append(new Monster[]
            {
                new Monster ( MonsterId : 1, Name : "Foo", MaxHp : 100 )
            }).Append(new Person[]
            {
                new Person { PersonId = 0, Age = 13, Gender = Gender.Male,   Name = "Dana Terry" },
                new Person { PersonId = 1, Age = 17, Gender = Gender.Male,   Name = "Kirk Obrien" },
                new Person { PersonId = 2, Age = 31, Gender = Gender.Male,   Name = "Wm Banks" },
                new Person { PersonId = 3, Age = 44, Gender = Gender.Male,   Name = "Karl Benson" },
                new Person { PersonId = 4, Age = 23, Gender = Gender.Male,   Name = "Jared Holland" },
                new Person { PersonId = 5, Age = 27, Gender = Gender.Female, Name = "Jeanne Phelps" },
                new Person { PersonId = 6, Age = 25, Gender = Gender.Female, Name = "Willie Rose" },
                new Person { PersonId = 7, Age = 11, Gender = Gender.Female, Name = "Shari Gutierrez" },
                new Person { PersonId = 8, Age = 63, Gender = Gender.Female, Name = "Lori Wilson" },
                new Person { PersonId = 9, Age = 34, Gender = Gender.Female, Name = "Lena Ramsey" },
            })
            .Append(new Quest[]
            {
                new Quest { Id= 1, Name = "foo", Cost = 10, RewardId = 100 },
                new Quest { Id= 2, Name = "bar", Cost = 20, RewardId = 101 },
                new Quest { Id= 3, Name = "baz", Cost = 30, RewardId = 100 },
                new Quest { Id= 4, Name = "too", Cost = 40, RewardId = 199 },
            })
            .Append(new Item[]
            {
                new Item { RewardId = 100 },
                new Item { RewardId = 101 },
                new Item { RewardId = 199 },
            })
            .Build();




            var db = new MemoryDatabase(bin);

            var result = db.Validate();


            Console.WriteLine(result.FormatFailedResults());
            //var db = new MemoryDatabase(File.ReadAllBytes("db.bin"));


            //Person person = db.PersonTable.FindByPersonId(10);


            //RangeView<Person> result = db.PersonTable.FindByGenderAndAge((Gender.Female, 23));


            //RangeView<Person> age1 = db.PersonTable.FindClosestByAge(31);


            //RangeView<Person> age2 = db.PersonTable.FindRangeByAge(20, 29);










        }
    }
}
