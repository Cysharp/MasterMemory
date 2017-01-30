using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory
{
    public class MasterMemoryQuery<TOuter>
    {
        readonly IEnumerable<TOuter> outerList;

        internal MasterMemoryQuery(IEnumerable<TOuter> outerList)
        {
            this.outerList = outerList;
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(OuterJoinCore(innerMemory, keySelector, resultSelector));
        }

        IEnumerable<TResult> OuterJoinCore<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var inner = innerMemory.FindOrDefault(keySelector(outer));
                yield return resultSelector(outer, inner);
            }
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(OuterJoinCore(innerMemory1, innerMemory2, keySelector, resultSelector));
        }

        IEnumerable<TResult> OuterJoinCore<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindOrDefault(key);
                var inner2 = innerMemory2.FindOrDefault(key);
                yield return resultSelector(outer, inner1, inner2);
            }
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(OuterJoinCore(innerMemory1, innerMemory2, innerMemory3, keySelector, resultSelector));
        }

        IEnumerable<TResult> OuterJoinCore<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindOrDefault(key);
                var inner2 = innerMemory2.FindOrDefault(key);
                var inner3 = innerMemory3.FindOrDefault(key);
                yield return resultSelector(outer, inner1, inner2, inner3);
            }
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(OuterJoinCore(innerMemory1, innerMemory2, innerMemory3, innerMemory4, keySelector, resultSelector));
        }

        IEnumerable<TResult> OuterJoinCore<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindOrDefault(key);
                var inner2 = innerMemory2.FindOrDefault(key);
                var inner3 = innerMemory3.FindOrDefault(key);
                var inner4 = innerMemory4.FindOrDefault(key);
                yield return resultSelector(outer, inner1, inner2, inner3, inner4);
            }
        }

        public MasterMemoryQuery<TResult> InnerJoin<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(InnerJoinJoinCore(innerMemory, keySelector, resultSelector));
        }

        IEnumerable<TResult> InnerJoinJoinCore<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                TInner inner;
                if (innerMemory.TryFind(keySelector(outer), out inner))
                {
                    yield return resultSelector(outer, inner);
                }
            }
        }

        public MasterMemoryQuery<TResult> GroupJoin<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner>, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(GroupJoinCore(innerMemory, keySelector, resultSelector));
        }

        IEnumerable<TResult> GroupJoinCore<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner>, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var inner = innerMemory.FindMany(keySelector(outer));
                yield return resultSelector(outer, inner);
            }
        }

        /// <summary>
        /// Complete the query mode.
        /// </summary>
        public IEnumerable<TOuter> AsEnumerable()
        {
            return outerList.AsEnumerable();
        }
    }
}
