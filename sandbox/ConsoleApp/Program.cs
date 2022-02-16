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
using System.Globalization;

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
    public MyEnum MyProperty { get; set; }

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

    public enum MyEnum
    {
        A, B, C
    }
}

[MemoryTable("item"), MessagePackObject(true)]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}

namespace ConsoleApp.Tables
{
    public sealed partial class MonsterTable
    {
        /* readonly */
        int maxHp;

        partial void OnAfterConstruct()
        {
            maxHp = All.Select(x => x.MaxHp).Max();
        }
    }
}

namespace ConsoleApp
{
    [MemoryTable("monster"), MessagePackObject(true)]
    public class Monster
    {
        [PrimaryKey]
        public int MonsterId { get; private set; }
        public string Name { get; private set; }
        public int MaxHp { get; private set; }

        public Monster(int MonsterId, string Name, int MaxHp)
        {
            this.MonsterId = MonsterId;
            this.Name = Name;
            this.MaxHp = MaxHp;
        }
    }

    [MemoryTable("enumkeytable"), MessagePackObject(true)]
    public class EnumKeyTable
    {
        [PrimaryKey]
        public Gender Gender { get; set; }
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
            var csv = @"monster_id,name,max_hp
    1,foo,100
    2,bar,200";
            var fileName = "monster";

            var builder = new DatabaseBuilder();

            var meta = MemoryDatabase.GetMetaDatabase();
            var table = meta.GetTableInfo(fileName);

            var tableData = new List<object>();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var reader = new TinyCsvReader(sr))
            {
                while ((reader.ReadValuesWithHeader() is Dictionary<string, string> values))
                {
                    // create data without call constructor
                    var data = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(table.DataType);

                    foreach (var prop in table.Properties)
                    {
                        if (values.TryGetValue(prop.NameSnakeCase, out var rawValue))
                        {
                            var value = ParseValue(prop.PropertyInfo.PropertyType, rawValue);
                            if (prop.PropertyInfo.SetMethod == null)
                            {
                                throw new Exception("Target property does not exists set method. If you use {get;}, please change to { get; private set; }, Type:" + prop.PropertyInfo.DeclaringType + " Prop:" + prop.PropertyInfo.Name);
                            }
                            prop.PropertyInfo.SetValue(data, value);
                        }
                        else
                        {
                            throw new KeyNotFoundException($"Not found \"{prop.NameSnakeCase}\" in \"{fileName}.csv\" header.");
                        }
                    }

                    tableData.Add(data);
                }
            }

            // add dynamic collection.
            builder.AppendDynamic(table.DataType, tableData);

            var bin = builder.Build();
            var database = new MemoryDatabase(bin, maxDegreeOfParallelism: Environment.ProcessorCount);
        }

        static object ParseValue(Type type, string rawValue)
        {
            if (type == typeof(string)) return rawValue;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrWhiteSpace(rawValue)) return null;
                return ParseValue(type.GenericTypeArguments[0], rawValue);
            }

            if (type.IsEnum)
            {
                var value = Enum.Parse(type, rawValue);
                return value;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    // True/False or 0,1
                    if (int.TryParse(rawValue, out var intBool))
                    {
                        return Convert.ToBoolean(intBool);
                    }
                    return Boolean.Parse(rawValue);
                case TypeCode.Char:
                    return Char.Parse(rawValue);
                case TypeCode.SByte:
                    return SByte.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Byte:
                    return Byte.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Int16:
                    return Int16.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.UInt16:
                    return UInt16.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Int32:
                    return Int32.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.UInt32:
                    return UInt32.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    return Int64.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return UInt64.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Single:
                    return Single.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return Double.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.Decimal:
                    return Decimal.Parse(rawValue, CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return DateTime.Parse(rawValue, CultureInfo.InvariantCulture);
                default:
                    if (type == typeof(DateTimeOffset))
                    {
                        return DateTimeOffset.Parse(rawValue, CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        return TimeSpan.Parse(rawValue, CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(Guid))
                    {
                        return Guid.Parse(rawValue);
                    }

                    // or other your custom parsing.
                    throw new NotSupportedException();
            }
        }

        // Non string escape, tiny reader with header.
        public class TinyCsvReader : IDisposable
        {
            static char[] trim = new[] { ' ', '\t' };

            readonly StreamReader reader;
            public IReadOnlyList<string> Header { get; private set; }

            public TinyCsvReader(StreamReader reader)
            {
                this.reader = reader;
                {
                    var line = reader.ReadLine();
                    if (line == null) throw new InvalidOperationException("Header is null.");

                    var index = 0;
                    var header = new List<string>();
                    while (index < line.Length)
                    {
                        var s = GetValue(line, ref index);
                        if (s.Length == 0) break;
                        header.Add(s);
                    }
                    this.Header = header;
                }
            }

            string GetValue(string line, ref int i)
            {
                var temp = new char[line.Length - i];
                var j = 0;
                for (; i < line.Length; i++)
                {
                    if (line[i] == ',')
                    {
                        i += 1;
                        break;
                    }
                    temp[j++] = line[i];
                }

                return new string(temp, 0, j).Trim(trim);
            }

            public string[] ReadValues()
            {
                var line = reader.ReadLine();
                if (line == null) return null;
                if (string.IsNullOrWhiteSpace(line)) return null;

                var values = new string[Header.Count];
                var lineIndex = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    var s = GetValue(line, ref lineIndex);
                    values[i] = s;
                }
                return values;
            }

            public Dictionary<string, string> ReadValuesWithHeader()
            {
                var values = ReadValues();
                if (values == null) return null;

                var dict = new Dictionary<string, string>();
                for (int i = 0; i < values.Length; i++)
                {
                    dict.Add(Header[i], values[i]);
                }

                return dict;
            }

            public void Dispose()
            {
                reader.Dispose();
            }
        }
    }


}


