using System.Linq.Expressions;

namespace SQLExtends.EFCore;

public static class ExpressionExtends
{
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "param");
        var leftVisitor = new ReplaceParameterVisitor(left.Parameters[0], parameter);
        var leftExpression = leftVisitor.Visit(left.Body);
        var rightVisitor = new ReplaceParameterVisitor(right.Parameters[0], parameter);
        var rightExpression = rightVisitor.Visit(right.Body);

        var combinedExpression = Expression.AndAlso(leftExpression, rightExpression);
        return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
    }

    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}