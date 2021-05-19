// <auto-generated />
#pragma warning disable CS0105
using ConsoleApp.Tables;
using ConsoleApp;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System;

namespace ConsoleApp.Tables
{
   public sealed partial class EnumKeyTableTable : TableBase<EnumKeyTable>, ITableUniqueValidate
   {
        public Func<EnumKeyTable, Gender> PrimaryKeySelector => primaryIndexSelector;
        readonly Func<EnumKeyTable, Gender> primaryIndexSelector;


        public EnumKeyTableTable(EnumKeyTable[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Gender;
            OnAfterConstruct();
        }

        partial void OnAfterConstruct();


        public EnumKeyTable FindByGender(Gender key)
        {
            return FindUniqueCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<Gender>.Default, key, true);
        }
        
        public bool TryFindByGender(Gender key, out EnumKeyTable result)
        {
            return TryFindUniqueCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<Gender>.Default, key, out result);
        }

        public EnumKeyTable FindClosestByGender(Gender key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<Gender>.Default, key, selectLower);
        }

        public RangeView<EnumKeyTable> FindRangeByGender(Gender min, Gender max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<Gender>.Default, min, max, ascendant);
        }


        void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)
        {
            ValidateUniqueCore(data, primaryIndexSelector, "Gender", resultSet);       
        }

        public static MasterMemory.Meta.MetaTable CreateMetaTable()
        {
            return new MasterMemory.Meta.MetaTable(typeof(EnumKeyTable), typeof(EnumKeyTableTable), "enumkeytable",
                new MasterMemory.Meta.MetaProperty[]
                {
                    new MasterMemory.Meta.MetaProperty(typeof(EnumKeyTable).GetProperty("Gender")),
                },
                new MasterMemory.Meta.MetaIndex[]{
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(EnumKeyTable).GetProperty("Gender"),
                    }, true, true, System.Collections.Generic.Comparer<Gender>.Default),
                });
        }

    }
}