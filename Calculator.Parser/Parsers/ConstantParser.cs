using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Парсер для математических констант.
    /// Поддерживает константы: pi (π), e (число Эйлера).
    /// </summary>
    public static class ConstantParser
    {
        public static readonly Parser<Expression> Constant =
            Parse.String("pi").Return(Expression.Constant(Math.PI)) 
                .Or(Parse.Char('e').Return(Expression.Constant(Math.E))); 
    }
}