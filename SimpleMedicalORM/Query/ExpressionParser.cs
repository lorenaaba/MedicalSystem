using System.Linq.Expressions;
using System.Reflection;
using SimpleMedicalORM.Mapping;

namespace SimpleMedicalORM.Query
{
    public class ExpressionParser
    {
        public static string ParseWhere<T>(Expression<Func<T, bool>> predicate)
        {
            return ParseExpression(predicate.Body, typeof(T));
        }

        private static string ParseExpression(Expression expression, Type entityType)
        {
            switch (expression)
            {
                case BinaryExpression binary:
                    return ParseBinaryExpression(binary, entityType);

                case MemberExpression member:
                    return ParseMemberExpression(member, entityType);

                case ConstantExpression constant:
                    return FormatValue(constant.Value);

                case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
                    return $"NOT ({ParseExpression(unary.Operand, entityType)})";

                default:
                    throw new NotSupportedException($"Expression type {expression.NodeType} not supported");
            }
        }

        private static string ParseBinaryExpression(BinaryExpression binary, Type entityType)
        {
            var left = ParseExpression(binary.Left, entityType);
            var right = ParseExpression(binary.Right, entityType);
            var op = GetOperator(binary.NodeType);

            return $"({left} {op} {right})";
        }

        private static string ParseMemberExpression(MemberExpression member, Type entityType)
        {
            if (member.Member is PropertyInfo property)
            {
                if (member.Expression?.Type == entityType)
                {
                    return EntityMapper.GetColumnName(property);
                }

                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                var value = getter();
                return FormatValue(value);
            }

            throw new NotSupportedException($"Member {member.Member.Name} not supported");
        }

        private static string GetOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Operator {nodeType} not supported")
            };
        }

        private static string FormatValue(object? value)
        {
            if (value == null)
                return "NULL";

            if (value is string str)
                return $"'{str.Replace("'", "''")}'";

            if (value is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";

            if (value is bool b)
                return b ? "TRUE" : "FALSE";

            return value.ToString() ?? "NULL";
        }
    }
}