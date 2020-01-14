 
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public partial class ValidatableSet<TElement>
    {
        public void Sequential(Expression<Func<TElement, SByte>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, SByte> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, SByte> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, Int16>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, Int16> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, Int16> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, Int32>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, Int32> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, Int32> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, Int64>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, Int64> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, Int64> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, Byte>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, Byte> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, Byte> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, UInt16>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, UInt16> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, UInt16> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, UInt32>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, UInt32> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, UInt32> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

        public void Sequential(Expression<Func<TElement, UInt64>> selector, bool distinct = false)
        {
            var f = selector.Compile(true);
            SequentialCore(f, () => selector.ToSpaceBodyString(), distinct);
        }

        public void Sequential(Func<TElement, UInt64> selector, string message, bool distinct = false)
        {
            SequentialCore(selector, () => " " + message, distinct);
        }

        void SequentialCore(Func<TElement, UInt64> selector, Func<string> message, bool distinct)
        {
            if (tableData.Count == 0) return;
            var data = tableData.OrderBy(selector).ToArray();

            var prev = selector(data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                var curr = selector(data[i]);
                if (distinct)
                {
                    if (prev == curr) continue;
                }

                if ((prev + 1) != curr)
                {
                    resultSet.AddFail(typeof(TElement), "Sequential failed:" + message() + ", value = " + (prev, curr) + ", " + BuildPkMessage(data[i]), data[i]);
                }

                prev = curr;
            }
        }

    }
}
