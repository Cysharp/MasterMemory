using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Tests.TestStructures
{
    [MemoryTable("quest_master"), MessagePackObject(true)]
    public class QuestMaster : IValidatable<QuestMaster>
    {
        [PrimaryKey]
        public int QuestId { get; set; }
        public string Name { get; set; }
        public int RewardItemId { get; set; }
        public int Cost { get; set; }

        public void Validate(IValidator<QuestMaster> validator)
        {
            var itemMaster = validator.GetReferenceSet<ItemMaster>();

            itemMaster.Exists(x => x.RewardItemId, x => x.ItemId);

            validator.Validate(x => x.Cost <= 100);
            validator.Validate(x => x.Cost >= 0, ">= 0!!!");

            validator.ValidateAction(() => this.Cost <= 1000);
            validator.ValidateAction(() => this.Cost >= -90, ">= -90!!!");

            if (validator.CallOnce())
            {
                var quests = validator.GetTableSet();
                quests.Unique(x => x.Name);
            }
        }
    }

    [MemoryTable("item_master"), MessagePackObject(true)]
    public class ItemMaster : IValidatable<ItemMaster>
    {
        [PrimaryKey]
        public int ItemId { get; set; }

        public void Validate(IValidator<ItemMaster> validator)
        {
        }
    }

    [MemoryTable("quest_master_empty"), MessagePackObject(true)]
    public class QuestMasterEmptyValidate
    {
        [PrimaryKey]
        public int QuestId { get; set; }
        public string Name { get; set; }
        public int RewardItemId { get; set; }
        public int Cost { get; set; }
    }

    [MemoryTable("item_master_empty"), MessagePackObject(true)]
    public class ItemMasterEmptyValidate
    {
        [PrimaryKey]
        public int ItemId { get; set; }
    }

    [MemoryTable("sequantial_master"), MessagePackObject(true)]
    public class SequentialCheckMaster : IValidatable<SequentialCheckMaster>
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int Cost { get; set; }

        public void Validate(IValidator<SequentialCheckMaster> validator)
        {
            if (validator.CallOnce())
            {
                var set = validator.GetTableSet();

                set.Sequential(x => x.Id);
                set.Sequential(x => x.Cost, true);
            }
        }
    }

    [MemoryTable("single_master"), MessagePackObject(true)]
    public class SingleMaster : IValidatable<SingleMaster>
    {
        public static int CalledValidateCount;
        public static int CalledOnceCount;

        [PrimaryKey]
        public int Id { get; set; }

        public void Validate(IValidator<SingleMaster> validator)
        {
            CalledValidateCount++;
            if (validator.CallOnce())
            {
                CalledOnceCount++;
            }
        }
    }

    [MemoryTable("fail"), MessagePackObject(true)]
    public class Fail : IValidatable<Fail>
    {
        [PrimaryKey]
        public int Id { get; set; }

        public void Validate(IValidator<Fail> validator)
        {
            validator.Fail("Failed Id:" + Id);
        }
    }
}
