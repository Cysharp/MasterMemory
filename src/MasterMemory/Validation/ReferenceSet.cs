using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class ReferenceSet<TElement, TReference>
    {
        readonly TElement item;
        readonly IReadOnlyList<TReference> referenceTable;
        readonly ValidateResult resultSet;
        readonly string pkName;
        readonly Delegate pkSelector;

        public IReadOnlyList<TReference> TableData => referenceTable;

        public ReferenceSet(TElement item, IReadOnlyList<TReference> referenceTable, ValidateResult resultSet, string pkName, Delegate pkSelector)
        {
            this.item = item;
            this.referenceTable = referenceTable;
            this.resultSet = resultSet;
            this.pkName = pkName;
            this.pkSelector = pkSelector;
        }

        public void Exists<TProperty>(Expression<Func<TElement, TProperty>> elementSelector, Expression<Func<TReference, TProperty>> referenceElementSelector)
        {
            Exists(elementSelector, referenceElementSelector, EqualityComparer<TProperty>.Default);
        }

        public void Exists<TProperty>(Expression<Func<TElement, TProperty>> elementSelector, Expression<Func<TReference, TProperty>> referenceElementSelector, EqualityComparer<TProperty> equalityComparer)
        {
            var f1 = elementSelector.Compile(true);
            var f2 = referenceElementSelector.Compile(true);

            var compareBase = f1(item);
            foreach (var refItem in referenceTable)
            {
                if (equalityComparer.Equals(compareBase, f2(refItem)))
                {
                    return;
                }
            }

            // not found, assert.
            var from = elementSelector.ToNameBodyString(typeof(TElement).Name);
            var to = referenceElementSelector.ToNameBodyString(typeof(TReference).Name);
            resultSet.AddFail(typeof(TElement), "Exists failed: " + from + " -> " + to + ", value = " + compareBase + ", " + BuildPkMessage(), item);
        }

        string BuildPkMessage()
        {
            var pk = pkSelector.DynamicInvoke(item).ToString();
            return $"PK({pkName}) = {pk}";
        }
    }
}