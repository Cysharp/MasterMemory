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

        public ValidatableSet(IReadOnlyList<TElement> tableData, ValidateResult resultSet)
        {
            this.tableData = tableData;
            this.resultSet = resultSet;
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
                    resultSet.AddFail(typeof(TElement), "Unique failed:" + selector.ToSpaceBodyString() + ", value = " + v);
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
                    resultSet.AddFail(typeof(TElement), "Unique failed: " + message + ", value = " + v);
                }
            }
        }
    }
}
