using Calculator.Parser.Parsers.OperatorParser;
using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Парсер для условного выражения if.
    /// Формат: if(условие, выражение_если_истина, выражение_если_ложь).
    /// </summary>
    public static class IfFunctionParser
    {
        public static readonly Parser<Expression> IfFunction =
            from ifKeyword in Parse.IgnoreCase("if")
            from openParen in Parse.Char('(')
            from condition in Parse.Ref(() => LogicalExprParser.LogicalExpr)
            from comma1 in Parse.Char(',').Token()
            from trueExpr in Parse.Ref(() => ExprParser.ExprUniversal)
            from comma2 in Parse.Char(',').Token()
            from falseExpr in Parse.Ref(() => ExprParser.ExprUniversal)
            from closeParen in Parse.Char(')')
            select Expression.Condition(condition, trueExpr, falseExpr);
    }
}