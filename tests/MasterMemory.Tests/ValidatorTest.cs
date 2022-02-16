using MasterMemory.Tests.TestStructures;
using FluentAssertions;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace MasterMemory.Tests
{
    public class ValidatorTest
    {
        readonly Xunit.Abstractions.ITestOutputHelper output;

#if UNITY_2018_3_OR_NEWER
        public ValidatorTest()
        {
            this.output = new Xunit.Abstractions.DebugLogTestOutputHelper();
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(MessagePackResolver.Instance);
        }
#else
        public ValidatorTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            this.output = output;
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(MessagePackResolver.Instance);
        }
#endif

        MemoryDatabase CreateDatabase(Fail[] data1)
        {

            var bin = new DatabaseBuilder()
                .Append(data1)
                .Build();

            return new MemoryDatabase(bin, internString: false);
        }

        MemoryDatabase CreateDatabase(SingleMaster[] data1)
        {

            var bin = new DatabaseBuilder()
                .Append(data1)
                .Build();

            return new MemoryDatabase(bin, internString: false);
        }

        MemoryDatabase CreateDatabase(SequentialCheckMaster[] data1)
        {

            var bin = new DatabaseBuilder()
                .Append(data1)
                .Build();

            return new MemoryDatabase(bin, internString: false);
        }

        MemoryDatabase CreateDatabase(QuestMaster[] data1, ItemMaster[] data2)
        {

            var bin = new DatabaseBuilder()
                .Append(data1)
                .Append(data2)
                .Build();

            return new MemoryDatabase(bin, internString: false);
        }

        MemoryDatabase CreateDatabase(QuestMasterEmptyValidate[] data1, ItemMasterEmptyValidate[] data2)
        {

            var bin = new DatabaseBuilder()
                .Append(data1)
                .Append(data2)
                .Build();

            return new MemoryDatabase(bin, internString: false);
        }

        [Fact]
        public void Empty()
        {
            var validateResult = CreateDatabase(new QuestMaster[]
            {
            }, new ItemMaster[]
            {
            }).Validate();

            validateResult.IsValidationFailed.Should().BeFalse();
            validateResult.FailedResults.Count.Should().Be(0);
        }

        [Fact]
        public void PKUnique()
        {
            var validateResult = CreateDatabase(new QuestMasterEmptyValidate[]
            {
                new QuestMasterEmptyValidate { QuestId = 1 },
                new QuestMasterEmptyValidate { QuestId = 2 },
                new QuestMasterEmptyValidate { QuestId = 1 },
                new QuestMasterEmptyValidate { QuestId = 4 },
                new QuestMasterEmptyValidate { QuestId = 4 },
            }, new ItemMasterEmptyValidate[]
            {
                new ItemMasterEmptyValidate { ItemId = 1 },
                new ItemMasterEmptyValidate { ItemId = 2 },
                new ItemMasterEmptyValidate { ItemId = 2 },
            }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());

            validateResult.IsValidationFailed.Should().BeTrue();
            validateResult.FailedResults.Count.Should().Be(3); // Q:1,4 + I:2
            var faileds = validateResult.FailedResults.OrderBy(x => x.Message).ToArray();

            faileds[0].Message.Should().Be("Unique failed: ItemId, value = 2");
            faileds[1].Message.Should().Be("Unique failed: QuestId, value = 1");
            faileds[2].Message.Should().Be("Unique failed: QuestId, value = 4");
        }

        // test IValidator

        /*
        public interface IValidator<T>
        {
            ValidatableSet<T> GetTableSet();
            ReferenceSet<T, TRef> GetReferenceSet<TRef>();
            void Validate(Expression<Func<T, bool>> predicate);
            void Validate(Func<T, bool> predicate, string message);
            void ValidateAction(Expression<Func<bool>> predicate);
            void ValidateAction(Func<bool> predicate, string message);
            void Fail(string message);
            bool CallOnce();
        }

        ReferenceSet.Exists
        ValidatableSet.Unique
        ValidatableSet.Sequential
    */

        [Fact]
        public void Exists()
        {
            var validateResult = CreateDatabase(new QuestMaster[]
            {
                new QuestMaster { QuestId = 1, RewardItemId = 1, Name = "foo" },
                new QuestMaster { QuestId = 2, RewardItemId = 3, Name = "bar" },
                new QuestMaster { QuestId = 3, RewardItemId = 2, Name = "baz" },
                new QuestMaster { QuestId = 4, RewardItemId = 5, Name = "tako"},
                new QuestMaster { QuestId = 5, RewardItemId = 4, Name = "nano"},
            }, new ItemMaster[]
            {
                new ItemMaster { ItemId = 1 },
                new ItemMaster { ItemId = 2 },
                new ItemMaster { ItemId = 3 },
            }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());
            validateResult.IsValidationFailed.Should().BeTrue();

            validateResult.FailedResults[0].Message.Should().Be("Exists failed: QuestMaster.RewardItemId -> ItemMaster.ItemId, value = 5, PK(QuestId) = 4");
            validateResult.FailedResults[1].Message.Should().Be("Exists failed: QuestMaster.RewardItemId -> ItemMaster.ItemId, value = 4, PK(QuestId) = 5");
        }

        [Fact]
        public void Unique()
        {
            var validateResult = CreateDatabase(new QuestMaster[]
            {
                new QuestMaster { QuestId = 1, Name = "foo" },
                new QuestMaster { QuestId = 2, Name = "bar" },
                new QuestMaster { QuestId = 3, Name = "bar" },
                new QuestMaster { QuestId = 4, Name = "tako" },
                new QuestMaster { QuestId = 5, Name = "foo" },
            }, new ItemMaster[]
            {
                new ItemMaster { ItemId = 0 }
            }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());
            validateResult.IsValidationFailed.Should().BeTrue();

            validateResult.FailedResults[0].Message.Should().Be("Unique failed: .Name, value = bar, PK(QuestId) = 3");
            validateResult.FailedResults[1].Message.Should().Be("Unique failed: .Name, value = foo, PK(QuestId) = 5");
        }

        [Fact]
        public void Sequential()
        {
            {
                var validateResult = CreateDatabase(new SequentialCheckMaster[]
                {
                    new SequentialCheckMaster { Id = 1, Cost = 10 },
                    new SequentialCheckMaster { Id = 2, Cost = 11 },
                    new SequentialCheckMaster { Id = 3, Cost = 11 },
                    new SequentialCheckMaster { Id = 4, Cost = 12 },
                }).Validate();
                output.WriteLine(validateResult.FormatFailedResults());
                validateResult.IsValidationFailed.Should().BeFalse();
            }
            {
                var validateResult = CreateDatabase(new SequentialCheckMaster[]
                {
                    new SequentialCheckMaster { Id = 1, Cost = 10 },
                    new SequentialCheckMaster { Id = 2, Cost = 11 },
                    new SequentialCheckMaster { Id = 3, Cost = 11 },
                    new SequentialCheckMaster { Id = 5, Cost = 13 },
                }).Validate();
                output.WriteLine(validateResult.FormatFailedResults());
                validateResult.IsValidationFailed.Should().BeTrue();

                validateResult.FailedResults[0].Message.Should().Be("Sequential failed: .Id, value = (3, 5), PK(Id) = 5");
                validateResult.FailedResults[1].Message.Should().Be("Sequential failed: .Cost, value = (11, 13), PK(Id) = 5");
            }
        }

        [Fact]
        public void CallOnce()
        {
            _ = CreateDatabase(new SingleMaster[]
            {
                new SingleMaster { Id = 1},
                new SingleMaster { Id = 2},
                new SingleMaster { Id = 3},
                new SingleMaster { Id = 4},
            }).Validate();


            SingleMaster.CalledValidateCount.Should().Be(4);
            SingleMaster.CalledOnceCount.Should().Be(1);
        }

        [Fact]
        public void Validate()
        {
            var validateResult = CreateDatabase(new QuestMaster[]
            {
                new QuestMaster { QuestId = 1, RewardItemId = 1, Name = "foo", Cost = -1 },
                new QuestMaster { QuestId = 2, RewardItemId = 3, Name = "bar", Cost = 99 },
                new QuestMaster { QuestId = 3, RewardItemId = 2, Name = "baz", Cost = 100 },
                new QuestMaster { QuestId = 4, RewardItemId = 3, Name = "tao", Cost = 101 },
                new QuestMaster { QuestId = 5, RewardItemId = 3, Name = "nao", Cost = 33 },
            }, new ItemMaster[]
            {
                new ItemMaster { ItemId = 1 },
                new ItemMaster { ItemId = 2 },
                new ItemMaster { ItemId = 3 },
            }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());
            validateResult.IsValidationFailed.Should().BeTrue();

            validateResult.FailedResults[0].Message.Should().Be("Validate failed: >= 0!!!, PK(QuestId) = 1");
            validateResult.FailedResults[1].Message.Should().Be("Validate failed: (this.Cost <= 100), Cost = 101, PK(QuestId) = 4");
        }

        [Fact]
        public void ValidateAction()
        {
            var validateResult = CreateDatabase(new QuestMaster[]
             {
                new QuestMaster { QuestId = 1, RewardItemId = 1, Name = "foo", Cost = -100 },
                new QuestMaster { QuestId = 2, RewardItemId = 3, Name = "bar", Cost = 99 },
                new QuestMaster { QuestId = 3, RewardItemId = 2, Name = "baz", Cost = 100 },
                new QuestMaster { QuestId = 4, RewardItemId = 3, Name = "tao", Cost = 1001 },
                new QuestMaster { QuestId = 5, RewardItemId = 3, Name = "nao", Cost = 33 },
             }, new ItemMaster[]
             {
                new ItemMaster { ItemId = 1 },
                new ItemMaster { ItemId = 2 },
                new ItemMaster { ItemId = 3 },
             }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());
            validateResult.IsValidationFailed.Should().BeTrue();

            var results = validateResult.FailedResults.Select(x => x.Message).Where(x => x.Contains("ValidateAction faile")).ToArray();

            results[0].Should().Be("ValidateAction failed: >= -90!!!, PK(QuestId) = 1");
            results[1].Should().Be("ValidateAction failed: (value(MasterMemory.Tests.TestStructures.QuestMaster).Cost <= 1000), PK(QuestId) = 4");
        }

        [Fact]
        public void Fail()
        {
            var validateResult = CreateDatabase(new Fail[]
            {
                new Fail { Id = 1},
                new Fail { Id = 2},
                new Fail { Id = 3},
            }).Validate();
            output.WriteLine(validateResult.FormatFailedResults());
            validateResult.IsValidationFailed.Should().BeTrue();

            var msg = validateResult.FailedResults.Select(x => x.Message).ToArray();
            msg[0].Should().Be("Failed Id:1, PK(Id) = 1");
            msg[1].Should().Be("Failed Id:2, PK(Id) = 2");
            msg[2].Should().Be("Failed Id:3, PK(Id) = 3");
        }
    }
}
