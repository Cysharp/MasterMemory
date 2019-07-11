using MasterMemory.Tests;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace MasterMemory.Tests.Tables
{
   public sealed partial class UserLevelTable : TableBase<UserLevel>
   {
        readonly Func<UserLevel, int> primaryIndexSelector;

        readonly UserLevel[] secondaryIndex0;
        readonly Func<UserLevel, int> secondaryIndex0Selector;

        public UserLevelTable(UserLevel[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Level;
            this.secondaryIndex0Selector = x => x.Exp;
            this.secondaryIndex0 = CloneAndSortBy(this.secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default);
        }

        public RangeView<UserLevel> SortByExp => new RangeView<UserLevel>(secondaryIndex0, 0, secondaryIndex0.Length, true);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public UserLevel FindByLevel(int key)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
				var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Level;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { return data[mid]; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            return default;
        }

        public UserLevel FindClosestByLevel(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<UserLevel> FindRangeByLevel(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

        public UserLevel FindByExp(int key)
        {
            return FindUniqueCoreInt(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, key);
        }

        public UserLevel FindClosestByExp(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<UserLevel> FindRangeByExp(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

    }
}