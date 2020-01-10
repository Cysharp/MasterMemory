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
    }
}
