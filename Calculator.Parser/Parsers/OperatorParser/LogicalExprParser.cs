using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    /// <summary>
    /// Парсер для логических выражений, который использует операторы сравнения.
    /// </summary>
    public static class LogicalExprParser
    {
        /// <summary>
        /// Парсер для логических выражений, использующий операции сравнения.
        /// </summary>
        public static readonly Parser<Expression> LogicalExpr =
            ComparisonExprParser.ComparisonExpr;  // Используем парсер для операций сравнения
    }
}