using Sprache;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    public static class ExprParser
    {
        // Основной парсер для обработки операций сложения и вычитания
        public static readonly Parser<Expression> ExprUniversal =
            Parse.ChainOperator(
                Parse.Char('+').Token().Return("Add")
                    .Or(Parse.Char('-').Token().Return("Subtract")),
                TermParser.TermUniversal,
                (op, left, right) => HandleOperation(op, left, right)
            );

        // Обработка операции в зависимости от типов операндов
        private static Expression HandleOperation(string op, Expression left, Expression right)
        {
            // Операции для обоих скалярных значений
            if (left.Type == typeof(double) && right.Type == typeof(double))
                return Expression.MakeBinary(
                    op == "Add" ? ExpressionType.Add : ExpressionType.Subtract,
                    left, right
                );

            // Операции для двух векторов
            if (left.Type == typeof(double[]) && right.Type == typeof(double[]))
                return CallVectorMethod(op, left, right);

            // Операции для смешанных типов (вектор и скаляр)
            return HandleScalarVectorOperation(op, left, right);
        }

        // Вызов метода для векторных операций
        private static Expression CallVectorMethod(string op, Expression left, Expression right)
        {
            var method = typeof(VectorOperations).GetMethod(op, new[] { typeof(double[]), typeof(double[]) });
            if (method == null)
                throw new InvalidOperationException($"Метод для операции {op} не найден.");

            return Expression.Call(method, left, right);
        }

        // Обработка операций для смешанных типов (вектор и скаляр)
        private static Expression HandleScalarVectorOperation(string op, Expression left, Expression right)
        {
            Expression scalarExpr = left.Type == typeof(double) ? left : right;
            Expression vectorExpr = left.Type == typeof(double) ? right : left;

            var method = typeof(VectorOperations).GetMethod(op, new[] { scalarExpr.Type, vectorExpr.Type });
            if (method == null)
                throw new InvalidOperationException($"Метод для операции {op} не найден.");

            return Expression.Call(method, scalarExpr, vectorExpr);
        }
    }
}
