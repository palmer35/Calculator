using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Парсер для операндов.
    /// Поддерживает числа, константы, выражения в скобках, вызовы функций, условные выражения и унарные отрицания.
    /// </summary>
    public static class OperandParser
    {
        /// <summary>
        /// Парсер для операндов.
        /// </summary>
        public static readonly Parser<Expression> Operand =
            Parse.Ref(() => NumberParser.Number) // Парсинг чисел
                .Or(Parse.Ref(() => ConstantParser.Constant)) // Парсинг констант
                .Or(Parse.Ref(() => ParenthesizedParser.Parenthesized)) // Парсинг выражений в скобках
                .Or(Parse.Ref(() => FunctionParser.Function)) // Парсинг вызовов функций
                .Or(Parse.Ref(() => IfFunctionParser.IfFunction)) // Парсинг условных выражений
                .Or(Parse.Ref(() => NegateParser.Negate)) // Парсинг унарных отрицаний
                .Or(Parse.Ref(() => VectorParser.Vector)) // Добавление парсера для векторов
                .Token(); // Игнорирование пробелов вокруг операнда

        /// <summary>
        /// Универсальный парсер для операндов, который поддерживает EvaluatedValue и векторные операции.
        /// </summary>
        public static readonly Parser<Expression> OperandUniversal =
            Parse.Ref(() => NumberParser.Number) // Парсинг чисел
                .Or(Parse.Ref(() => ConstantParser.Constant)) // Парсинг констант
                .Or(Parse.Ref(() => ParenthesizedParser.Parenthesized)) // Парсинг выражений в скобках
                .Or(Parse.Ref(() => FunctionParser.Function)) // Парсинг вызовов функций
                .Or(Parse.Ref(() => IfFunctionParser.IfFunction)) // Парсинг условных выражений
                .Or(Parse.Ref(() => NegateParser.Negate)) // Парсинг унарных отрицаний
                .Or(Parse.Ref(() => VectorParser.VectorUniversal)) // Универсальный парсер для векторов
                .Token(); // Игнорирование пробелов вокруг операнда
    }
}
