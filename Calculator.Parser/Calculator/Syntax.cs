using Calculator.Parser.Parsers.OperatorParser;
using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Calculator
{
    public class Syntax
    {
        /// <summary>
        /// Парсер для лямбда-выражения, возвращающего значение типа double (скаляр) или типа double[] (вектор).
        /// </summary>
        public static readonly Parser<Expression<Func<object>>> ParseUniversalLambda =
            from body in Parse.Ref(() => ExprParser.ExprUniversal).End()
            select Expression.Lambda<Func<object>>(ConvertToObject(body));

        /// <summary>
        /// Преобразует результат выражения к object, если он не object.
        /// </summary>
        private static Expression ConvertToObject(Expression body)
        {
            if (body.Type == typeof(object))
                return body;

            if (body.Type == typeof(double) || body.Type == typeof(double[]))
                return Expression.Convert(body, typeof(object));

            throw new InvalidOperationException($"Неподдерживаемый тип выражения: {body.Type}");
        }
    }
}
