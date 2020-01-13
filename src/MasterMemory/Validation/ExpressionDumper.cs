using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MasterMemory.Validation
{
    internal class ExpressionDumper<T> : ExpressionVisitor
    {
        ParameterExpression param;
        T target;

        public Dictionary<string, object> Members { get; private set; }

        public ExpressionDumper(T target, ParameterExpression param)
        {
            this.target = target;
            this.param = param;
            this.Members = new Dictionary<string, object>();
        }

        protected override System.Linq.Expressions.Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == param && !Members.ContainsKey(node.Member.Name))
            {
                var accessor = new ReflectAccessor(target, node.Member.Name);
                Members.Add(node.Member.Name, accessor.GetValue());
            }

            return base.VisitMember(node);
        }

        public static string DumpMemberValues(T item, Expression<Func<T, bool>> predicate)
        {
            var dumper = new ExpressionDumper<T>(item, predicate.Parameters.Single());
            return dumper.VisitAndFormat(predicate);
        }

        public string VisitAndFormat(Expression expression)
        {
            Visit(expression);
            return string.Join(", ", Members.Select(kvp => kvp.Key + " = " + kvp.Value));
        }

        private class ReflectAccessor
        {
            public Func<object> GetValue { get; private set; }
            public Action<object> SetValue { get; private set; }

            public ReflectAccessor(T target, string name)
            {
                var field = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    GetValue = () => field.GetValue(target);
                    SetValue = value => field.SetValue(target, value);
                    return;
                }

                var prop = typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    GetValue = () => prop.GetValue(target, null);
                    SetValue = value => prop.SetValue(target, value, null);
                    return;
                }

                throw new ArgumentException(string.Format("\"{0}\" not found : Type <{1}>", name, typeof(T).Name));
            }
        }
    }
}
