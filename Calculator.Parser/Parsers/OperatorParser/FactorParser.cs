using Sprache;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    /// <summary>
    /// Парсер для выражений, содержащих операцию возведения в степень.
    /// </summary>
    public static class FactorParser
    {
        /// <summary>
        /// Парсер для операции возведения в степень с использованием стандартных скалярных типов.
        /// </summary>
        public static readonly Parser<Expression> Factor =
            Parse.ChainOperator(
                Parse.Char('^').Return(ExpressionType.Power),  // Оператор возведения в степень
                OperandParser.Operand,  // Ожидаемый операнд (левая и правая часть)
                (op, left, right) => Expression.MakeBinary(op, left, right)  // Стандартная бинарная операция
            );

        /// <summary>
        /// Универсальный парсер для операции возведения в степень, который работает с типом Expression.
        /// </summary>
        public static readonly Parser<Expression> FactorUniversal =
            Parse.ChainOperator(
                Parse.Char('^').Token().Return("Power"),  // Используем строку для обозначения операции
                OperandParser.OperandUniversal,  // Универсальный парсер операндов
                (_, left, right) =>
                {
                    // Преобразуем операнды в выражение для вызова Math.Pow для возведения в степень
                    var method = typeof(Math).GetMethod("Pow", new[] { typeof(double), typeof(double) });
                    Debug.Assert(method != null, nameof(method) + " != null");
                    return Expression.Call(method, left, right);  // Вызов Math.Pow с операндами
                }
            );
    }
}