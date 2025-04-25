using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    public static class VectorParser
    {
        /// <summary>
        /// Парсер для векторов.
        /// </summary>
        public static readonly Parser<Expression> Vector =
            from openBracket in Parse.Char('[')
            from values in Parse.Ref(() => OperandParser.Operand).DelimitedBy(Parse.Char(',').Token())
            from closeBracket in Parse.Char(']')
            select Expression.NewArrayInit(typeof(double), values);  // Массив чисел

        /// <summary>
        /// Универсальный парсер для векторов.
        /// </summary>
        public static readonly Parser<Expression> VectorUniversal =
            from openBracket in Parse.Char('[')
            from values in Parse.Ref(() => OperandParser.OperandUniversal).DelimitedBy(Parse.Char(',').Token())
            from closeBracket in Parse.Char(']')
            select Expression.NewArrayInit(typeof(double), values);  // Массив чисел
    }
}
