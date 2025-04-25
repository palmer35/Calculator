using Calculator.WPF.Services.Calculation;
using ClassLibrary1;

namespace Calculator.Parser.Tests.Tests.ExpressionEvaluation
{
    [TestClass]
    public class VariableExpressionTests : TestBase
    {
        private CalculationController _controller;

        [TestInitialize]
        public void Setup() => _controller = CreateProcessor();

        [DataTestMethod]
        // Арифметика со скалярными переменными
        [DataRow("a + b * c", 2.0, 3.0, 4.0, null, null, null, new[] { 14.0 })]
        [DataRow("(a - b) * c", 5.0, 3.0, 4.0, null, null, null, new[] { 8.0 })]
        [DataRow("a / b + c", 10.0, 2.0, 1.0, null, null, null, new[] { 6.0 })]

        // Проверка приоритета операций
        [DataRow("(a + b) * c", 2.0, 3.0, 4.0, null, null, null, new[] { 20.0 })]  // Скобки меняют порядок

        // Векторная арифметика с переменными
        [DataRow("sum[a] + mean[b]", 0, 0, 0, new[] { 1.0, 2.0, 3.0 }, new[] { 4.0, 5.0, 6.0 }, null, new[] { 11.0 })]
        [DataRow("min(a, b)", 5.0, 3.0, 0, null, null, null, new[] { 3.0 })]  // min с переменными
        [DataRow("max(a, b)", 1.0, 5.0, 0, null, null, null, new[] { 5.0 })]  // max с переменными

        [DataRow("mean[a] + sum[b]", 0, 0, 0, new[] { 2.0, 4.0 }, new[] { 1.0, 3.0 }, null, new[] { 7.0 })]

        // Функции с переменными
        [DataRow("sqrt(a)", 25.0, 0, 0, null, null, null, new[] { 5.0 })]
        [DataRow("pow(a, b)", 2.0, 3.0, 0, null, null, null, new[] { 8.0 })]
        [DataRow("abs(a)", -15.0, 0, 0, null, null, null, new[] { 15.0 })]
        [DataRow("cos(a) + sin(b)", 0.0, 0.0, 0.0, null, null, null, new[] { 1.0 })]

        // Условные выражения с переменными
        [DataRow("if(a > b, 10, 20)", 5.0, 3.0, 0.0, null, null, null, new[] { 10.0 })]  // Условие с переменными
        [DataRow("if(mean[a] > 2, sum[b], sqrt(c))", 0, 0, 16.0, new[] { 1.0, 3.0, 5.0 }, new[] { 2.0, 4.0, 3.0 }, null, new[] { 9.0 })]

        // Комбинированные выражения с переменными
        [DataRow("sum[a] * pow(b, 3) / 2", 0, 2.0, 0, new[] { 1.0, 2.0 }, null, null, new[] { 12.0 })]  // Комбинированное выражение с переменными
        [DataRow("sqrt(max(a, b)) + min(c, 7)", 9.0, 16.0, 3.0, null, null, null, new[] { 7.0 })]  // max, min, sqrt с переменными
        [DataRow("if(a > b, mean[c], sqrt(c))", 5.0, 3.0, 0, null, null, new[] { 2.0, 4.0, 6.0 }, new[] { 4.0 })]  // Условие с переменными и функциями
        [DataRow("(sum[a] + pow(b, 3)) / 2", 0, 2.0, 0, new[] { 1.0, 2.0, 3.0 }, null, null, new[] { 7.0 })]  // Сложение sum и pow с переменными
        [DataRow("abs(a) * mean[b]", -5.0, 0.0, 0.0, null, new[] { 10.0, 20.0 }, null, new[] { 75.0 })]  // abs и mean с переменными

        public void Calculate_ExpressionsWithVariables_ReturnsCorrectResult(
            string input,
            double aScalar,
            double bScalar,
            double cScalar,
            double[]? aVector,
            double[]? bVector,
            double[]? cVector,
            double[] expected)
        {
            var scalarVars = new Dictionary<string, double>
            {
                { "a", aScalar },
                { "b", bScalar },
                { "c", cScalar }
            };

            var vectorVars = CreateVectorVariables(aVector, bVector, cVector);

            try
            {
                var result = _controller.Calculate(input, scalarVars, vectorVars);

                var actual = result as double[]
                    ?? throw new AssertFailedException($"Ожидался результат типа double[], но получено: {result.GetType().Name}");

                CollectionAssert.AreEqual(
                expected,
                actual,
                $"Ошибка в выражении \"{input}\": ожидалось [{string.Join(", ", expected)}], " +
                $"получено [{string.Join(", ", actual)}]");

            }
            catch (CalculatorException ex)
            {
                Assert.Fail($"Ошибка вычисления: {ex.Message}");
            }
        }

        private Dictionary<string, double[]> CreateVectorVariables(double[]? a, double[]? b, double[]? c)
        {
            var vectorVars = new Dictionary<string, double[]>();
            if (a != null) vectorVars.Add("a", a);
            if (b != null) vectorVars.Add("b", b);
            if (c != null) vectorVars.Add("c", c);
            return vectorVars;
        }
    }
}