using System;
using System.Linq.Expressions;

namespace MasterMemory.Validation
{
    public class ExpressionParameterNameModifier : ExpressionVisitor
    {
        readonly ParameterExpression modifyTarget;
        readonly ParameterExpression replaceExpression;

        public ExpressionParameterNameModifier(ParameterExpression modifyTarget, ParameterExpression replaceExpression)
        {
            this.modifyTarget = modifyTarget;
            this.replaceExpression = replaceExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == modifyTarget)
            {
                return replaceExpression;
            }

            return base.VisitParameter(node);
        }
    }

    public static class ExpressionParameterNameModifyExtensions
    {
        public static string ToThisBodyString<T>(this Expression<Func<T, bool>> predicate)
        {
            var newNameParameter = Expression.Parameter(typeof(T), "this");
            var newExpression = new ExpressionParameterNameModifier(predicate.Parameters[0], newNameParameter).Visit(predicate);
            return (newExpression as Expression<Func<T, bool>>).Body.ToString();
        }

        public static string ToSpaceBodyString<T, TProperty>(this Expression<Func<T, TProperty>> selector)
        {
            var newNameParameter = Expression.Parameter(typeof(T), " ");
            var newExpression = new ExpressionParameterNameModifier(selector.Parameters[0], newNameParameter).Visit(selector);
            return (newExpression as Expression<Func<T, TProperty>>).Body.ToString();
        }

        public static string ToNameBodyString<T, TProperty>(this Expression<Func<T, TProperty>> selector, string newName)
        {
            var newNameParameter = Expression.Parameter(typeof(T), newName);
            var newExpression = new ExpressionParameterNameModifier(selector.Parameters[0], newNameParameter).Visit(selector);
            return (newExpression as Expression<Func<T, TProperty>>).Body.ToString();
        }
    }
}
