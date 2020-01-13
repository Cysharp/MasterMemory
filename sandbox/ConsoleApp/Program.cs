using MasterMemory;
using System.Linq;
using MessagePack;
using System;
using System.IO;
using System.Buffers;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

// IValidatableを実装すると検証対象になる
[MemoryTable("quest_master"), MessagePackObject(true)]
public class Quest : IValidatable<Quest>
{
    // UniqueKeyの場合はValidate時にデフォルトで重複かの検証がされる
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public int RewardId { get; set; }
    public int Cost { get; set; }

    void IValidatable<Quest>.Validate(IValidator<Quest> validator)
    {
        // 外部キー的に参照したいコレクションを取り出せる
        var items = validator.GetReferenceSet<Item>();

        // RewardIdが0以上のとき(0は報酬ナシのための特別なフラグとするため入力を許容する)
        if (this.RewardId > 0)
        {
            // Itemsのマスタに必ず含まれてなければ検証エラー（エラーが出ても続行はしてすべての検証結果を出す)
            items.Exists(x => x.RewardId, x => x.ItemId);
        }

        // コストは10..20でなければ検証エラー
        validator.Validate(x => x.Cost >= 10);
        validator.Validate(x => x.Cost <= 20);

        // 以下で囲った部分は一度しか呼ばれないため、データセット全体の検証をしたい時に使える
        if (validator.CallOnce())
        {
            var quests = validator.GetTableSet();
            // インデックス生成したもの以外のユニークどうかの検証(0は重複するため除いておく)
            quests.Where(x => x.RewardId != 0).Unique(x => x.RewardId);
        }
    }
}

[MemoryTable("item"), MessagePackObject(true)]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}

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

    [MemoryTable(nameof(Test1))]
    public class Test1
    {
        [PrimaryKey]
        public int Id { get; set; }
    }

    [MessagePackObject(false)]
    [MemoryTable(nameof(Test2))]
    public class Test2
    {
        [PrimaryKey]
        public int Id { get; set; }
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
                new Quest { Id= 3, Name = "baz", Cost = 30, RewardId = 0 },
                new Quest { Id= 3, Name = "too", Cost = 40, RewardId = 0 },
            })
            .Append(new Item[]
            {
                new Item { ItemId = 100 },
                new Item { ItemId = 101 },
                new Item { ItemId = 199 },
            })
            .Build();




            var db = new MemoryDatabase(bin);


            // テーブル情報、プロパティ情報、インデックス情報が取れるので自由に加工する
            var metaDb = MetaMemoryDatabase.GetMetaDatabase();
            foreach (var table in metaDb.GetTableInfos())
            {
                // CSVのヘッダ生成
                var sb = new StringBuilder();
                foreach (var prop in table.Properties)
                {
                    if (sb.Length != 0) sb.Append(",");

                    // そのまま, LowerCamelCase, SnakeCaseに変換した名前が取得可能
                    sb.Append(prop.NameSnakeCase);
                }
                Console.WriteLine(sb.ToString());
                File.WriteAllText(table.TableName + ".csv", sb.ToString(), new UTF8Encoding(false));
            }



            // 検証結果取得。データベースの構築自体は検証とは無関係に構築ができるので、
            // （開発時用などに）不整合のまま出してもいいし、(リリース時では)弾くなどご自由に。
            var validateResult = db.Validate();
            if (validateResult.IsValidationFailed)
            {
                // 検証失敗データを文字列形式でフォーマットして出力
                Console.WriteLine(validateResult.FormatFailedResults());

                // List<(Type, string)> で検証データを取得して、自分でカスタムで出力することも可能
                // MDやHTMLに整形してSlackやレポーターに投げるなど自由に
                // validateResult.FailedResults
            }

            // new MetaMemoryDatabase()

        }
    }
}


