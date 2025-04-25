using Calculator.Parser.Parsers.OperatorParser;
using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Парсер для выражений, заключенных в скобки.
    /// Поддерживает выражения вида: (5 + 3), (x * y)
    /// </summary>
    public static class ParenthesizedParser
    {
        /// <summary>
        /// Парсер для выражений в скобках.
        /// </summary>
        public static readonly Parser<Expression> Parenthesized =
            from openParen in Parse.Char('(') 
            from operand in Parse.Ref(() => LogicalExprParser.LogicalExpr) // Парсинг выражения внутри скобок
            from closeParen in Parse.Char(')') 
            select operand;
    }
}