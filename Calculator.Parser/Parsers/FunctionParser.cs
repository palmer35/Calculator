using Calculator.Parser.Calculator;
using Calculator.Parser.Parsers.OperatorParser;
using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    public static class FunctionParser
    {
        private static readonly FunctionEvaluator FunctionHandler = new();

        // Парсер для функции с круглыми скобками
        private static readonly Parser<Expression> FunctionWithParentheses =
            from nameWithPos in Parse.Letter.AtLeastOnce().Text()
                .Select(text => new PositionedValue<string>(text))
                .Token()
                .Positioned()
            from openParen in Parse.Char('(').Token()
            from args in Parse.Ref(() => ExprParser.ExprUniversal)  // Используем универсальный парсер
                .DelimitedBy(Parse.Char(',').Token())
            from closeParen in Parse.Char(')')
            select CreateFunctionExpression(nameWithPos.Value, args.ToArray(), nameWithPos.Position);

        // Парсер для функции с квадратными скобками
        private static readonly Parser<Expression> FunctionWithBrackets =
            from nameWithPos in Parse.Letter.AtLeastOnce().Text()
                .Select(text => new PositionedValue<string>(text))
                .Token()
                .Positioned()
            from openBracket in Parse.Char('[').Token()
            from args in Parse.Ref(() => ExprParser.ExprUniversal)  // Используем универсальный парсер
                .DelimitedBy(Parse.Char(',').Token())
            from closeBracket in Parse.Char(']')
            select CreateFunctionExpression(nameWithPos.Value, args.ToArray(), nameWithPos.Position);

        // Объединяем оба парсера (круглые и квадратные скобки)
        public static readonly Parser<Expression> Function =
            FunctionWithParentheses.Or(FunctionWithBrackets);

        // Метод для создания выражения функции
        private static Expression CreateFunctionExpression(string name, Expression[] args, int position)
        {
            var errorPositions = new Dictionary<string, int>
            {
                { name, position }
            };

            return FunctionHandler.GetFunctionExpression(name, args, errorPositions);
        }
    }
}
