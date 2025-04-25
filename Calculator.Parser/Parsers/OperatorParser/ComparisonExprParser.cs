using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    public static class ComparisonExprParser
    {
        // Основной парсер — универсальный
        public static readonly Parser<Expression> ComparisonExpr =
            Parse.ChainOperator(
                // Операторы для сравнения
                Parse.String("<=").Return(ExpressionType.LessThanOrEqual)
                    .Or(Parse.String(">=").Return(ExpressionType.GreaterThanOrEqual))
                    .Or(Parse.String("==").Return(ExpressionType.Equal))
                    .Or(Parse.String("!=").Return(ExpressionType.NotEqual))
                    .Or(Parse.Char('<').Return(ExpressionType.LessThan))
                    .Or(Parse.Char('>').Return(ExpressionType.GreaterThan)),
                ExprParser.ExprUniversal,
                (op, left, right) =>
                {
                    // Если оба аргумента — вектора
                    if (left.Type == typeof(double[]) && right.Type == typeof(double[]))
                    {
                        var method = op switch
                        {
                            ExpressionType.Equal => typeof(VectorOperations).GetMethod(nameof(VectorOperations.Equal)),
                            ExpressionType.NotEqual => typeof(VectorOperations).GetMethod(nameof(VectorOperations.NotEqual)),
                            ExpressionType.LessThan => typeof(VectorOperations).GetMethod(nameof(VectorOperations.LessThan)),
                            ExpressionType.LessThanOrEqual => typeof(VectorOperations).GetMethod(nameof(VectorOperations.LessThanOrEqual)),
                            ExpressionType.GreaterThan => typeof(VectorOperations).GetMethod(nameof(VectorOperations.GreaterThan)),
                            ExpressionType.GreaterThanOrEqual => typeof(VectorOperations).GetMethod(nameof(VectorOperations.GreaterThanOrEqual)),
                            _ => throw new NotSupportedException($"Оператор {op} не поддерживается для векторов")
                        };

                        if (method == null)
                        {
                            throw new InvalidOperationException($"Метод для оператора {op} не найден в классе VectorOperations.");
                        }

                        return Expression.Call(
                            method,
                            Expression.Convert(left, typeof(double[])),
                            Expression.Convert(right, typeof(double[]))
                        );
                    }

                    // Вектор и скаляр
                    if (left.Type == typeof(double[]) && right.Type == typeof(double))
                    {
                        var method = typeof(VectorOperations).GetMethod(nameof(VectorOperations.CompareVectorToScalar))!;
                        return Expression.Call(method, Expression.Convert(left, typeof(double[])), Expression.Convert(right, typeof(double)), Expression.Constant(op));
                    }

                    if (left.Type == typeof(double) && right.Type == typeof(double[]))
                    {
                        var method = typeof(VectorOperations).GetMethod(nameof(VectorOperations.CompareScalarToVector))!;
                        return Expression.Call(method, Expression.Convert(left, typeof(double)), Expression.Convert(right, typeof(double[])), Expression.Constant(op));
                    }

                    // Скалярное сравнение
                    return Expression.MakeBinary(op, left, right);
                }
            );
    }
}
