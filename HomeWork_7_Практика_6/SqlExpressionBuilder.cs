using System.Linq.Expressions;
using System.Reflection;

namespace MyORMLibrary
{
    public class SqlExpressionBuilder
    {
        private int _paramIndex = 0;
        public Dictionary<string, object> Parameters { get; } = new();

        public string Build(Expression expression)
        {
            return Visit(expression);
        }

        private string Visit(Expression exp)
        {
            return exp switch
            {
                LambdaExpression l => Visit(l.Body),

                BinaryExpression b => VisitBinary(b),

                MemberExpression m => VisitMember(m),

                ConstantExpression c => VisitConstant(c),

                MethodCallExpression mc => VisitMethodCall(mc),

                UnaryExpression u => Visit(u.Operand),

                _ => throw new NotSupportedException($"Неподдерживаемое выражение: {exp.GetType().Name}")
            };
        }

        private string VisitBinary(BinaryExpression b)
        {
            string left = Visit(b.Left);
            string right = Visit(b.Right);
            string op = b.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Оператор {b.NodeType} не поддерживается")
            };

            return $"({left} {op} {right})";
        }

        private string VisitMember(MemberExpression m)
        {
            if (m.Expression is ParameterExpression)
            {
                return $"{m.Member.Name.ToLower()}";
            }
            object value = null;
            try
            {
                if(m.Expression is ConstantExpression constExpr)
                {
                    var container = constExpr.Value;
                    switch (m.Member)
                    {
                        case FieldInfo fi:
                            value = fi.GetValue(container);
                            break;
                        case PropertyInfo pi:
                            value = pi.GetValue(container);
                            break;
                        default:
                            throw new NotSupportedException("Неподдерживаемый тип элемента в выражении");
                    }
                    return AddParam(value);
                }
                
                var lambda = Expression.Lambda(Expression.Convert(m, typeof(object)));
                var getter = (Func<object>)lambda.Compile();
                value = getter();
                return AddParam(value);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Не удалось извлечь значение из MemberExpression '{m}'.", ex);
            }
        }

        private string VisitConstant(ConstantExpression c)
        {
            string name = $"@p{_paramIndex++}";
            Parameters[name] = c.Value;
            return name;
        }

        private string VisitMethodCall(MethodCallExpression mc)
        {
            // LIKE: из него x.Name.Contains("abc")
            if (mc.Method.Name == "Contains" && mc.Object != null)
            {
                var column = Visit(mc.Object).ToLower();
                var value = GetValue(mc.Arguments[0]);
                string name = AddParam($"%{value}%");

                return $"({column} LIKE {name})";
            }

            // из StartsWith в LIKE 'abc%'
            if (mc.Method.Name == "StartsWith")
            {
                var column = Visit(mc.Object).ToLower();
                var value = GetValue(mc.Arguments[0]);
                string name = AddParam($"{value}%");
                return $"({column} LIKE {name})";
            }

            // из EndsWith в LIKE '%abc'
            if (mc.Method.Name == "EndsWith")
            {
                var column = Visit(mc.Object).ToLower();
                var value = GetValue(mc.Arguments[0]);
                string name = AddParam($"%{value}");
                return $"({column} LIKE {name})";
            }

            throw new NotSupportedException($"Метод {mc.Method.Name} не поддерживается");
        }

        private string AddParam(object value)
        {
            string name = $"@p{_paramIndex++}";
            Parameters[name] = value;
            return name;
        }

        private object GetValue(Expression exp)
        {
            if (exp is ConstantExpression c)
                return c.Value;

            // На случай: x => x.Age > var chtoto

            var lambda = Expression.Lambda(exp);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
    }
}
