using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Calculator
{
    /// <summary>
    /// Отвечает за разбор строки выражения и построение выражения LINQ Expression.
    /// </summary>
    public class ExpressionParser
    {
        /// <summary>
        /// Разбирает строку выражения в Expression.
        /// </summary>
        /// <param name="input">Строковое математическое выражение.</param>
        public Expression<Func<object>> Parse(string input)
        {
            var result = Syntax.ParseUniversalLambda(new Input(input));

            if (!result.WasSuccessful)
                throw new Exception($"Ошибка парсинга выражения: {result.Message}");

            // Конвертируем тело выражения в object
            var convertedBody = Expression.Convert(result.Value.Body, typeof(object));

            return Expression.Lambda<Func<object>>(convertedBody);
        }
    }
}