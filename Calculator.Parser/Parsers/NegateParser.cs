using Calculator.Parser.Parsers.OperatorParser;
using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Парсер для унарного оператора отрицания (-).
    /// Поддерживает выражения вида: -x, -(5 + 3).
    /// </summary>
    public static class NegateParser
    {
        public static readonly Parser<Expression> Negate =
            from negative in Parse.Char('-')
            from expression in Parse.Ref(() => FactorParser.Factor)
            select Expression.Negate(expression);
    }
}