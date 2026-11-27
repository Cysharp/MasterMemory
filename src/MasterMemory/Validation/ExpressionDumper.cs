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
                Members.Add(node.Member.Name, GetValue(node.Member));
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

        // The member is the one the expression names: a FieldInfo or a PropertyInfo that the compiler put into the
        // tree. It is read as it is. Looking it up again by its name on typeof(T) would be reflection that the trimmer
        // and the Native AOT compiler cannot follow, and would miss a member declared in a base class besides.
        object GetValue(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.GetValue(target);
                case PropertyInfo property:
                    return property.GetValue(target, null);
                default:
                    throw new ArgumentException(string.Format("\"{0}\" is neither a field nor a property : Type <{1}>", member.Name, typeof(T).Name));
            }
        }
    }
}
