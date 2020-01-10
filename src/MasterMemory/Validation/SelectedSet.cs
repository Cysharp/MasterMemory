using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class SelectedSet<TElement, TReference, TProperty>
    {
        IAssertionCollector colelctor;
        TElement element;
        TReference[] references;
        Expression<Func<TReference, TProperty>> referenceSelector;
        //public void Exists(string propertyName, TProperty item)
        //{
        //}
        public void Exists(Expression<Func<TProperty>> selector)
        {
            // Any(selector, EqualityComparer<TProperty>.Default);
        }
        public void Any(Expression<Func<TProperty>> selector, IEqualityComparer<TProperty> equalityComparer)
        {
            var prop = selector.Compile(preferInterpretation: true).Invoke(); // .Invoke(element);
            var refSelector = referenceSelector.Compile(preferInterpretation: true);
            var found = false;
            foreach (var item in references)
            {
                if (equalityComparer.Equals(prop, refSelector(item)))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                colelctor.AddFailed("hogehogehugahuga");
            }
        }
        public void All(Expression<Func<TElement, TProperty>> selector, IEqualityComparer<TProperty> equalityComparer)
        {
            var prop = selector.Compile(preferInterpretation: true).Invoke(element);
            var refSelector = referenceSelector.Compile(preferInterpretation: true);
            foreach (var item in references)
            {
                if (!equalityComparer.Equals(prop, refSelector(item)))
                {
                    colelctor.AddFailed("hugahugatakotako");
                    break;
                }
            }
        }
    }
}
