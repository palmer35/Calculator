using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    /// <summary>
    /// Парсер для арифметических выражений, содержащих операции умножения и деления.
    /// Поддерживает скаляры, векторы и смешанные типы.
    /// </summary>
    public static class TermParser
    {
        /// <summary>
        /// Стандартный парсер терминов: только скалярные и векторные операции.
        /// </summary>
        public static readonly Parser<Expression> Term =
            Parse.ChainOperator(
                Parse.Chars('*', '×', '⋅').Return("Multiply")
                    .Or(Parse.Chars('/', '÷').Return("Divide"))
                   .Or(Parse.Char('%').Token().Return("Modulo")),   
                FactorParser.Factor,
                ApplyOperation
            );

        /// <summary>
        /// Универсальный парсер терминов: расширен для смешанных типов (скаляр + вектор).
        /// </summary>
        public static readonly Parser<Expression> TermUniversal =
            Parse.ChainOperator(
                Parse.Chars('*', '×', '⋅').Token().Return("Multiply")
           .Or(Parse.Chars('/', '÷').Token().Return("Divide"))
           .Or(Parse.Char('%').Token().Return("Modulo")), 
                FactorParser.FactorUniversal,
                ApplyOperation
            );

        /// <summary>
        /// Применяет арифметическую операцию к двум выражениям с учетом их типов.
        /// </summary>
        private static Expression ApplyOperation(string op, Expression left, Expression right)
        {
            var opName = op switch
            {
                "Multiply" => "Multiply",
                "Divide" => "Divide",
                "Modulo" => "Modulo",
                _ => throw new InvalidOperationException($"Неизвестная операция: {op}")
            };

            // double * double
            if (left.Type == typeof(double) && right.Type == typeof(double))
            {
                return opName switch
                {
                    "Multiply" => Expression.Multiply(left, right),
                    "Divide" => Expression.Divide(left, right),
                    "Modulo" => Expression.Modulo(left, right),
                    _ => throw new InvalidOperationException($"Операция {opName} не поддерживается для скаляров.")
                };
            }

            // double[] * double[]
            if (left.Type == typeof(double[]) && right.Type == typeof(double[]))
            {
                var method = typeof(VectorOperations).GetMethod(opName, new[] { typeof(double[]), typeof(double[]) });
                return Expression.Call(method!, left, right);
            }

            // double * double[]
            if (left.Type == typeof(double) && right.Type == typeof(double[]))
            {
                var method = typeof(VectorOperations).GetMethod(opName, new[] { typeof(double), typeof(double[]) });
                return Expression.Call(method!, left, right);
            }

            // double[] * double
            if (left.Type == typeof(double[]) && right.Type == typeof(double))
            {
                var method = typeof(VectorOperations).GetMethod(opName, new[] { typeof(double[]), typeof(double) });
                return Expression.Call(method!, left, right);
            }

            throw new InvalidOperationException($"Операция {opName} не поддерживается для типов: {left.Type} и {right.Type}");
        }
    }
}
