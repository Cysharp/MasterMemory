using System;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class FilteredSet<TElement, TReference>
    {
        public SelectedSet<TElement, TReference, TProperty> Select<TProperty>(Expression<Func<TReference, TProperty>> selector)
        {
            return new SelectedSet<TElement, TReference, TProperty>();
        }
        // Distinct???
    }
}
