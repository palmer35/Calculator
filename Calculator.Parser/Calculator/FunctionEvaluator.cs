using Calculator.Parser.Parsers.OperatorParser;
using ClassLibrary1;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Calculator.Parser.Calculator
{
    /// <summary>
    /// Класс для обработки функций в математическом выражении.
    /// Содержит методы для работы с математическими, агрегатными и условными функциями.
    /// </summary>
    public class FunctionEvaluator
    {
        private readonly Dictionary<string, Func<Expression[], Expression>> _functions;

        public FunctionEvaluator()
        {
            _functions = new Dictionary<string, Func<Expression[], Expression>>(StringComparer.OrdinalIgnoreCase)
            {
                { "sqrt", HandleMathFunction("Sqrt", 1) },
                { "abs", HandleMathFunction("Abs", 1) },
                { "sin", HandleMathFunction("Sin", 1) },
                { "cos", HandleMathFunction("Cos", 1) },
                { "min", HandleMathFunction("Min", 2) },
                { "max", HandleMathFunction("Max", 2) },
                { "pow", HandleMathFunction("Pow", 2) },
                { "log",  HandleMathFunction("Log", 2) }, //наш log
                { "sum", HandleAggregateFunction("Sum") },
                { "mean", HandleMean },
                { "fact", HandleFact }, //наш fact
                { "if", HandleIf }
            };
        }

        /// <summary>
        /// Получить выражение для функции.
        /// </summary>
        /// <param name="name">Имя функции.</param>
        /// <param name="args">Аргументы функции.</param>
        /// <param name="errorPosition">Позиция ошибки для подробной диагностики.</param>
        /// <returns>Возвращает выражение функции.</returns>
        public Expression GetFunctionExpression(string name, Expression[] args, Dictionary<string, int>? errorPosition)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                try
                {
                    return function(args);
                }
                catch (CalculatorException ex) when (ex.ErrorPosition == -1)
                {
                    throw new CalculatorException(ex.Message, errorPosition);
                }
            }

            throw new CalculatorException($"Неизвестная функция: {name}", errorPosition);
        }

        /// <summary>
        /// Создаёт функцию для работы с математическими функциями.
        /// </summary>
        private static Func<Expression[], Expression> HandleMathFunction(string methodName, int arity)
        {
            return args =>
            {
                if (args.Length != arity)
                    throw new CalculatorException($"Функция {methodName} требует {arity} аргумент(ов)");

                var methodInfo = typeof(Math).GetMethod(methodName, Enumerable.Repeat(typeof(double), arity).ToArray());
                if (methodInfo == null)
                    throw new InvalidOperationException($"Метод Math.{methodName} не найден");

                // Обработка случаев с массивами и скалярами
                return arity == 1
                    ? HandleUnaryFunction(methodInfo, args[0])
                    : HandleBinaryFunction(methodInfo, args[0], args[1]);
            };
        }

        /// <summary>
        /// Применяет унарную математическую функцию к массиву или скаляру.
        /// </summary>
        private static Expression HandleUnaryFunction(MethodInfo method, Expression arg)
        {
            return arg.Type.IsArray
                ? ApplyUnaryToArray(arg, method)
                : Expression.Call(method, arg);
        }

        /// <summary>
        /// Применяет бинарную математическую функцию для двух аргументов (может быть как для массивов, так и для скалярных значений).
        /// </summary>
        private static Expression HandleBinaryFunction(MethodInfo method, Expression a, Expression b)
        {
            if (!a.Type.IsArray && !b.Type.IsArray)
                return Expression.Call(method, a, b);
            else if (a.Type.IsArray && b.Type.IsArray)
                return ApplyElementWiseBinary(a, b, method);
            else if (a.Type.IsArray)
                return ApplyScalarToArray(a, b, method, scalarFirst: false);
            else
                return ApplyScalarToArray(b, a, method, scalarFirst: true);
        }

        /// <summary>
        /// Применяет унарную функцию к каждому элементу массива.
        /// </summary>
        private static Expression ApplyUnaryToArray(Expression array, MethodInfo method)
        {
            var length = Expression.ArrayLength(array);
            var resultArray = Expression.Variable(typeof(double[]), "result");
            var loopVar = Expression.Variable(typeof(int), "i");
            var breakLabel = Expression.Label("Break");

            return Expression.Block(
                new[] { resultArray, loopVar },
                Expression.Assign(resultArray, Expression.NewArrayBounds(typeof(double), length)),
                Expression.Assign(loopVar, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(loopVar, length),
                        Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(resultArray, loopVar),
                                Expression.Call(method, Expression.ArrayIndex(array, loopVar))
                            ),
                            Expression.PostIncrementAssign(loopVar)
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                resultArray
            );
        }

        /// <summary>
        /// Применяет бинарную функцию к каждому элементу двух массивов.
        /// </summary>
        private static Expression ApplyElementWiseBinary(Expression a, Expression b, MethodInfo method)
        {
            var lenA = Expression.ArrayLength(a);
            var lenB = Expression.ArrayLength(b);
            var minLength = Expression.Variable(typeof(int), "minLength");
            var i = Expression.Variable(typeof(int), "i");
            var result = Expression.Variable(typeof(List<double>), "result");
            var breakLabel = Expression.Label("Break");

            return Expression.Block(
                new[] { minLength, i, result },
                Expression.Assign(minLength, Expression.Condition(Expression.LessThan(lenA, lenB), lenA, lenB)),
                Expression.Assign(i, Expression.Constant(0)),
                Expression.Assign(result, Expression.New(typeof(List<double>))),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(i, minLength),
                        Expression.Block(
                            Expression.Call(
                                result,
                                typeof(List<double>).GetMethod("Add")!,
                                Expression.Call(method,
                                    Expression.ArrayIndex(a, i),
                                    Expression.ArrayIndex(b, i))
                            ),
                            Expression.PostIncrementAssign(i)
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                Expression.Call(typeof(Enumerable), "ToArray", new[] { typeof(double) }, result)
            );
        }

        /// <summary>
        /// Применяет скалярную функцию ко всем элементам массива.
        /// </summary>
        private static Expression ApplyScalarToArray(Expression array, Expression scalar, MethodInfo method, bool scalarFirst)
        {
            var length = Expression.ArrayLength(array);
            var resultArray = Expression.Variable(typeof(double[]), "result");
            var loopVar = Expression.Variable(typeof(int), "i");
            var breakLabel = Expression.Label("Break");

            return Expression.Block(
                new[] { resultArray, loopVar },
                Expression.Assign(resultArray, Expression.NewArrayBounds(typeof(double), length)),
                Expression.Assign(loopVar, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(loopVar, length),
                        Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(resultArray, loopVar),
                                scalarFirst
                                    ? Expression.Call(method, scalar, Expression.ArrayIndex(array, loopVar))
                                    : Expression.Call(method, Expression.ArrayIndex(array, loopVar), scalar)
                            ),
                            Expression.PostIncrementAssign(loopVar)
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                resultArray
            );
        }

        /// <summary>
        /// Функция для вычисления среднего значения.
        /// </summary>
        private static Expression HandleMean(Expression[] args)
        {
            if (args.Length < 1)
                throw new CalculatorException("Функция mean требует хотя бы один аргумент");

            var sum = args.Aggregate((left, right) => Expression.Add(left, right));
            var count = Expression.Constant(args.Length);
            var countAsDouble = Expression.Convert(count, typeof(double));
            return Expression.Divide(sum, countAsDouble);
        }

        /// <summary>
        /// Функция условного выражения (if).
        /// </summary>
        private static Expression HandleIf(Expression[] args)
        {
            if (args.Length != 3)
                throw new CalculatorException("Функция if требует три аргумента");

            var condition = args[0];
            var trueExpr = args[1];
            var falseExpr = args[2];

            // Преобразование массивов в скалярные значения для условия
            if (condition.Type.IsArray)
            {
                var conditionArray = Expression.Call(
                    typeof(Enumerable), "Cast", new[] { typeof(double) }, condition
                );
                condition = Expression.Call(
                    typeof(Enumerable), "First", new[] { typeof(double) }, conditionArray
                );
            }

            return Expression.Condition(condition, trueExpr, falseExpr);
        }

        private static Expression HandleFact(Expression[] args)
        {
            if (args.Length != 1)
                throw new CalculatorException("Функция fact требует ровно один аргумент");

            var arg = args[0];
            // Ищем метод VectorOperations.Fact(double) или VectorOperations.Fact(double[])
            var methodInfo = typeof(VectorOperations)
                .GetMethod("Fact", new[] { arg.Type });
            if (methodInfo == null)
                throw new InvalidOperationException($"Метод VectorOperations.Fact({arg.Type.Name}) не найден.");

            // Возвращаем прямой вызов этой перегрузки
            return Expression.Call(methodInfo, arg);
        }


        /// <summary>
        /// Создаёт функцию для агрегатных операций (например, Sum).
        /// </summary>
        private static Func<Expression[], Expression> HandleAggregateFunction(string methodName)
        {
            return args =>
            {
                if (args.Length == 0)
                    throw new CalculatorException($"Функция {methodName} требует хотя бы один аргумент");

                var enumerables = new List<Expression>();

                foreach (var arg in args)
                {
                    if (arg.Type.IsArray)
                    {
                        var castCall = Expression.Call(
                            typeof(Enumerable),
                            "Cast",
                            new[] { typeof(double) },
                            arg
                        );
                        enumerables.Add(castCall);
                    }
                    else
                    {
                        var converted = Expression.Convert(arg, typeof(double));
                        var array = Expression.NewArrayInit(typeof(double), converted);
                        enumerables.Add(array);
                    }
                }

                Expression combined = enumerables.First();
                foreach (var next in enumerables.Skip(1))
                {
                    combined = Expression.Call(
                        typeof(Enumerable),
                        "Concat",
                        new[] { typeof(double) },
                        combined,
                        next
                    );
                }

                var method = typeof(Enumerable).GetMethod(methodName, new[] { typeof(IEnumerable<double>) });
                Debug.Assert(method != null, nameof(method) + " != null");
                return Expression.Call(null, method, combined);
            };
        }
    }
}

