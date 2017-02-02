using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory
{
    public class MasterMemoryQuery<TOuter> : IEnumerable<TOuter>
    {
        readonly IEnumerable<TOuter> outerList;

        internal MasterMemoryQuery(IEnumerable<TOuter> outerList)
        {
            this.outerList = outerList;
        }

        // Join

        public MasterMemoryQuery<TResult> Join<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(JoinCore(innerMemory, keySelector, resultSelector));
        }

        IEnumerable<TResult> JoinCore<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var inner = innerMemory.FindOrDefault(keySelector(outer));
                yield return resultSelector(outer, inner);
            }
        }

        public MasterMemoryQuery<TResult> Join<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(JoinCore(innerMemory1, innerMemory2, keySelector, resultSelector));
        }

        IEnumerable<TResult> JoinCore<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindOrDefault(key);
                var inner2 = innerMemory2.FindOrDefault(key);
                yield return resultSelector(outer, inner1, inner2);
            }
        }

        public MasterMemoryQuery<TResult> Join<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(JoinCore(innerMemory1, innerMemory2, innerMemory3, keySelector, resultSelector));
        }

        IEnumerable<TResult> JoinCore<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
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

        public MasterMemoryQuery<TResult> Join<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(JoinCore(innerMemory1, innerMemory2, innerMemory3, innerMemory4, keySelector, resultSelector));
        }

        IEnumerable<TResult> JoinCore<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
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

        // OuterJoin

        public MasterMemoryQuery<TResult> OuterJoin<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory, keySelector, resultSelector, true));
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, keySelector, resultSelector, true));
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, innerMemory3, keySelector, resultSelector, true));
        }

        public MasterMemoryQuery<TResult> OuterJoin<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, innerMemory3, innerMemory4, keySelector, resultSelector, true));
        }

        // InnerJoin

        public MasterMemoryQuery<TResult> InnerJoin<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory, keySelector, resultSelector, false));
        }

        public MasterMemoryQuery<TResult> InnerJoin<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, keySelector, resultSelector, false));
        }

        public MasterMemoryQuery<TResult> InnerJoin<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, innerMemory3, keySelector, resultSelector, false));
        }

        public MasterMemoryQuery<TResult> InnerJoin<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(FlatJoinCore(innerMemory1, innerMemory2, innerMemory3, innerMemory4, keySelector, resultSelector, false));
        }

        IEnumerable<TResult> FlatJoinCore<TInner, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner> innerMemory1, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner, TResult> resultSelector, bool defaultIfEmpty)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1Many = DefaultIfEmpty(innerMemory1.FindMany(key), defaultIfEmpty);

                foreach (var inner1 in inner1Many)
                {
                    yield return resultSelector(outer, inner1);
                }
            }
        }

        IEnumerable<TResult> FlatJoinCore<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TResult> resultSelector, bool defaultIfEmpty)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1Many = DefaultIfEmpty(innerMemory1.FindMany(key), defaultIfEmpty);
                var inner2Many = DefaultIfEmpty(innerMemory2.FindMany(key), defaultIfEmpty);

                foreach (var inner1 in inner1Many)
                {
                    foreach (var inner2 in inner2Many)
                    {
                        yield return resultSelector(outer, inner1, inner2);
                    }
                }
            }
        }

        IEnumerable<TResult> FlatJoinCore<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TResult> resultSelector, bool defaultIfEmpty)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1Many = DefaultIfEmpty(innerMemory1.FindMany(key), defaultIfEmpty);
                var inner2Many = DefaultIfEmpty(innerMemory2.FindMany(key), defaultIfEmpty);
                var inner3Many = DefaultIfEmpty(innerMemory3.FindMany(key), defaultIfEmpty);

                foreach (var inner1 in inner1Many)
                {
                    foreach (var inner2 in inner2Many)
                    {
                        foreach (var inner3 in inner3Many)
                        {
                            yield return resultSelector(outer, inner1, inner2, inner3);
                        }
                    }
                }
            }
        }

        IEnumerable<TResult> FlatJoinCore<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, TInner1, TInner2, TInner3, TInner4, TResult> resultSelector, bool defaultIfEmpty)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1Many = DefaultIfEmpty(innerMemory1.FindMany(key), defaultIfEmpty);
                var inner2Many = DefaultIfEmpty(innerMemory2.FindMany(key), defaultIfEmpty);
                var inner3Many = DefaultIfEmpty(innerMemory3.FindMany(key), defaultIfEmpty);
                var inner4Many = DefaultIfEmpty(innerMemory4.FindMany(key), defaultIfEmpty);

                foreach (var inner1 in inner1Many)
                {
                    foreach (var inner2 in inner2Many)
                    {
                        foreach (var inner3 in inner3Many)
                        {
                            foreach (var inner4 in inner4Many)
                            {
                                yield return resultSelector(outer, inner1, inner2, inner3, inner4);
                            }
                        }
                    }
                }
            }
        }

        IEnumerable<T> DefaultIfEmpty<T>(RangeView<T> view, bool defaultIfEmpty)
        {
            return (defaultIfEmpty) ? view.DefaultIfEmpty() : view;
        }

        // GroupJoin

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

        public MasterMemoryQuery<TResult> GroupJoin<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(GroupJoinCore(innerMemory1, innerMemory2, keySelector, resultSelector));
        }

        IEnumerable<TResult> GroupJoinCore<TInner1, TInner2, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindMany(key);
                var inner2 = innerMemory2.FindMany(key);
                yield return resultSelector(outer, inner1, inner2);
            }
        }

        public MasterMemoryQuery<TResult> GroupJoin<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, RangeView<TInner3>, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(GroupJoinCore(innerMemory1, innerMemory2, innerMemory3, keySelector, resultSelector));
        }

        IEnumerable<TResult> GroupJoinCore<TInner1, TInner2, TInner3, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, RangeView<TInner3>, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindMany(key);
                var inner2 = innerMemory2.FindMany(key);
                var inner3 = innerMemory3.FindMany(key);
                yield return resultSelector(outer, inner1, inner2, inner3);
            }
        }

        public MasterMemoryQuery<TResult> GroupJoin<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, RangeView<TInner3>, RangeView<TInner4>, TResult> resultSelector)
        {
            return new MasterMemoryQuery<TResult>(GroupJoinCore(innerMemory1, innerMemory2, innerMemory3, innerMemory4, keySelector, resultSelector));
        }

        IEnumerable<TResult> GroupJoinCore<TInner1, TInner2, TInner3, TInner4, TJoinKey, TResult>(IMemoryFinder<TJoinKey, TInner1> innerMemory1, IMemoryFinder<TJoinKey, TInner2> innerMemory2, IMemoryFinder<TJoinKey, TInner3> innerMemory3, IMemoryFinder<TJoinKey, TInner4> innerMemory4, Func<TOuter, TJoinKey> keySelector, Func<TOuter, RangeView<TInner1>, RangeView<TInner2>, RangeView<TInner3>, RangeView<TInner4>, TResult> resultSelector)
        {
            foreach (var outer in outerList)
            {
                var key = keySelector(outer);
                var inner1 = innerMemory1.FindMany(key);
                var inner2 = innerMemory2.FindMany(key);
                var inner3 = innerMemory3.FindMany(key);
                var inner4 = innerMemory4.FindMany(key);
                yield return resultSelector(outer, inner1, inner2, inner3, inner4);
            }
        }

        public IEnumerator<TOuter> GetEnumerator()
        {
            return outerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return outerList.GetEnumerator();
        }
    }

    public static class MasterMemoryQueryEnumerableExtensions
    {
        public static MasterMemoryQuery<T> AsMasterMemoryQuery<T>(this IEnumerable<T> source)
        {
            return new MasterMemoryQuery<T>(source);
        }

        // If you needs IEnumerable<T>.Distinct, use Ix or AnonymousComparer or others... 
        public static IEnumerable<T> Distinct<T, TKey>(this MasterMemoryQuery<T> source, Func<T, TKey> compareKeySelector)
        {
            var comparer = new EqualityComparer<T>(
                (x, y) =>
                {
                    if (object.ReferenceEquals(x, y)) return true;
                    if (x == null || y == null) return false;

                    return compareKeySelector(x).Equals(compareKeySelector(y));
                },
                obj =>
                {
                    if (obj == null) return 0;

                    return compareKeySelector(obj).GetHashCode();
                });

            return source.Distinct(comparer);
        }

        class EqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> equals;
            private readonly Func<T, int> getHashCode;

            public EqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
            {
                this.equals = equals;
                this.getHashCode = getHashCode;
            }

            public bool Equals(T x, T y)
            {
                return equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return getHashCode(obj);
            }
        }
    }
}