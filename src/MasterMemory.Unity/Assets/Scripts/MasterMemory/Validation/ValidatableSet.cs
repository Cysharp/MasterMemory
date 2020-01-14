using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public partial class ValidatableSet<TElement>
    {
        readonly IReadOnlyList<TElement> tableData;
        readonly ValidateResult resultSet;
        readonly string pkName;
        readonly Delegate pkSelector;

        public ValidatableSet(IReadOnlyList<TElement> tableData, ValidateResult resultSet, string pkName, Delegate pkSelector)
        {
            this.tableData = tableData;
            this.resultSet = resultSet;
            this.pkName = pkName;
            this.pkSelector = pkSelector;
        }

        public IReadOnlyList<TElement> TableData => tableData;

        public void Unique<TProperty>(Expression<Func<TElement, TProperty>> selector)
        {
            Unique(selector, EqualityComparer<TProperty>.Default);
        }

        public void Unique<TProperty>(Expression<Func<TElement, TProperty>> selector, IEqualityComparer<TProperty> equalityComparer)
        {
            var f = selector.Compile(true);

            var set = new HashSet<TProperty>(equalityComparer);
            foreach (var item in tableData)
            {
                var v = f(item);
                if (!set.Add(v))
                {
                    resultSet.AddFail(typeof(TElement), "Unique failed:" + selector.ToSpaceBodyString() + ", value = " + v + ", " + BuildPkMessage(item), item);
                }
            }
        }

        public void Unique<TProperty>(Func<TElement, TProperty> selector, string message)
        {
            Unique(selector, EqualityComparer<TProperty>.Default, message);
        }

        public void Unique<TProperty>(Func<TElement, TProperty> selector, IEqualityComparer<TProperty> equalityComparer, string message)
        {
            var set = new HashSet<TProperty>(equalityComparer);
            foreach (var item in tableData)
            {
                var v = selector(item);
                if (!set.Add(v))
                {
                    resultSet.AddFail(typeof(TElement), "Unique failed: " + message + ", value = " + v + ", " + BuildPkMessage(item), item);
                }
            }
        }

        public ValidatableSet<TElement> Where(Func<TElement, bool> predicate)
        {
            return new ValidatableSet<TElement>(tableData.Where(predicate).ToArray(), resultSet, pkName, pkSelector);
        }

        string BuildPkMessage(TElement item)
        {
            var pk = pkSelector.DynamicInvoke(item).ToString();
            return $"PK({pkName}) = {pk}";
        }
    }
}
