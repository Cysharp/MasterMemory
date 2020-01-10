using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class ValidatableSet<TElement>
    {
        readonly IReadOnlyList<TElement> tableData;

        public ValidatableSet(IReadOnlyList<TElement> tableData)
        {
            this.tableData = tableData;
        }

        public IReadOnlyList<TElement> TableData => tableData;

        public void Unique<TProperty>(Expression<Func<TElement, TProperty>> selector, IEqualityComparer<TProperty> equalityComparer)
        {
            var func = selector.Compile(preferInterpretation: true);
            Unique(func, equalityComparer, "hogehogemogemoge");
        }

        public void Unique<TProperty>(Func<TElement, TProperty> selector, IEqualityComparer<TProperty> equalityComparer, string message)
        {
            var set = new HashSet<TProperty>(equalityComparer);

            foreach (var item in tableData)
            {
                if (!set.Add(selector(item)))
                {
                    // TODO:assert duplicate
                }
            }
        }

        public void Sequential<TProperty>(Expression<Func<TElement, TProperty>> selector, bool distinct = false)
        {
        }
    }
}
