using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    /// <summary>
    /// Класс для реализации парсеров с учетом приоритета операторов.
    /// </summary>
    public static class OperatorPrecedence
    {
        /// <summary>
        /// Парсер для факторов (высший приоритет: числа, скобки, функции, отрицание).
        /// </summary>
        public static readonly Parser<Expression> Factor =
            Parse.ChainOperator(
                Parse.Char('^').Return(ExpressionType.Power), 
                OperandParser.Operand, 
                (op, left, right) => Expression.MakeBinary(op, left, right) // Создание бинарного выражения
            );

        /// <summary>
        /// Парсер для терминов (средний приоритет: умножение, деление).
        /// </summary>
        public static readonly Parser<Expression> Term =
            Parse.ChainOperator(
                Parse.Chars('*', '×', '⋅').Return(ExpressionType.Multiply) 
                    .Or(Parse.Chars('/', '÷').Return(ExpressionType.Divide)),
                Factor, 
                (op, left, right) =>
                {
                    // Проверка деления на ноль
                    if (op == ExpressionType.Divide && right is ConstantExpression rightConst && (double)(rightConst.Value ?? throw new InvalidOperationException()) == 0)
                    {
                        throw new DivideByZeroException("Деление на ноль недопустимо.");
                    }

                    // Создание бинарного выражения
                    return Expression.MakeBinary(op, left, right);
                }
            );

        /// <summary>
        /// Парсер для выражений (низший приоритет: сложение, вычитание).
        /// </summary>
        public static readonly Parser<Expression> Expr =
            Parse.ChainOperator(
                Parse.Char('+').Return(ExpressionType.Add) 
                    .Or(Parse.Char('-').Return(ExpressionType.Subtract)), 
                Term, 
                (op, left, right) => Expression.MakeBinary(op, left, right) // Создание бинарного выражения
            );

        /// <summary>
        /// Парсер для сравнений.
        /// </summary>
        public static readonly Parser<Expression> ComparisonExpr =
            Parse.ChainOperator(
                Parse.String("<=").Return(ExpressionType.LessThanOrEqual)
                    .Or(Parse.String(">=").Return(ExpressionType.GreaterThanOrEqual)) 
                    .Or(Parse.String("==").Return(ExpressionType.Equal)) 
                    .Or(Parse.String("!=").Return(ExpressionType.NotEqual)) 
                    .Or(Parse.Char('<').Return(ExpressionType.LessThan)) 
                    .Or(Parse.Char('>').Return(ExpressionType.GreaterThan)), 
                Expr, 
                (op, left, right) => Expression.MakeBinary(op, left, right) // Создание бинарного выражения
            );

        /// <summary>
        /// Парсер для логических выражений.
        /// </summary>
        public static readonly Parser<Expression> LogicalExpr =
            ComparisonExpr; // Логические выражения обрабатываются на уровне ComparisonExpr
    }
}