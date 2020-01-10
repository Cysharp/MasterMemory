using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class ReferenceSet<TElement, TReference>
    {
        readonly TElement item;
        readonly IReadOnlyList<TReference> referenceTable;

        public ReferenceSet(TElement item, IReadOnlyList<TReference> referenceTable)
        {
            this.item = item;
            this.referenceTable = referenceTable;
        }

        /// <summary>
        /// Shorthand of Select().Any();
        /// </summary>
        public void Exists<TProperty>(Expression<Func<TReference, TProperty>> referenceElementSelector, Expression<Func<TElement, TProperty>> elementSelector)
        {
            //Select(referenceElementSelector).Exists(elementSelector);
        }
        public SelectedSet<TElement, TReference, TProperty> Select<TProperty>(Expression<Func<TReference, TProperty>> selector)
        {
            return new SelectedSet<TElement, TReference, TProperty>();
        }
        // Where
    }
}
