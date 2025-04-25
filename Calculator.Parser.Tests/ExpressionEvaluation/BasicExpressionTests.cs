using Calculator.WPF.Services.Calculation;

namespace Calculator.Parser.Tests.Tests.ExpressionEvaluation
{
    [TestClass]
    public class BasicExpressionTests : TestBase
    {
        private CalculationController _controller;

        [TestInitialize]
        public void Setup() => _controller = CreateProcessor();

        [DataTestMethod]
        // Базовые арифметические операции
        [DataRow("2 + 3 * 4", new[] { 14.0 })]
        [DataRow("(5 - 3) * 4", new[] { 8.0 })]
        [DataRow("10 / 2 + 1", new[] { 6.0 })]
        [DataRow("1.1+1,1", new[] { 2.2 })]

        // Математические функции
        [DataRow("sqrt(25) + pow(2, 3)", new[] { 13.0 })]  // Комбинированная функция
        [DataRow("abs(-15) + cos(0) * sin(0)", new[] { 15.0 })]  // Математика с тригонометрией
        [DataRow("pow(2, 3) - sqrt(16)", new[] { 4.0 })]  // Степени и квадратные корни
        [DataRow("sqrt(abs(-25))", new[] { 5.0 })]  // Вложенные функции

        // Векторные операции
        [DataRow("sum[1, 2, 3] + mean[2, 4, 6]", new[] { 10.0 })]  // Операции с суммой и средним
        [DataRow("max([3, 4, 5], [2, 7, 2]) - min([1, 2, 3], [1, 1, 5])", new[] { 2.0, 6.0, 2.0 })]  // Операции с минимумом и максимумом
        [DataRow("max(5, 7) * min(3, 4)", new[] { 21.0 })]  // Векторное умножение
        [DataRow("sum[1, 2, 3] * mean[10, 20, 30]", new[] { 120.0 })]  // Умножение суммы на среднее

        // Условные выражения
        [DataRow("if(5 > 3, sum[1, 2], mean[2, 4])", new[] { 3.0 })]  // Условие с суммой
        [DataRow("if([2] < [5], 10, 20)", new[] { 10.0 })]  // Условие с векторами
        [DataRow("if(mean[1, 2, 3] > 2, sum[4, 5], sqrt(9))", new[] { 3.0 })]  // Условие с функцией
        [DataRow("if([10] > [5], sqrt(16), pow(2, 4))", new[] { 4.0 })]  // Условие с разными функциями

        // Комбинированные операции
        [DataRow("sum[1, 2, 3] * pow(2, 3) / 2 + sqrt(25)", new[] { 29.0 })]  // Сложные комбинированные операции
        [DataRow("sqrt(max(9, 16)) + min(3, 7) * abs(-10)", new[] { 34.0 })]  // Вложенные операции
        [DataRow("if([5] > [3], mean[2, 4, 6], sqrt(25)) + abs(-5)", new[] { 9.0 })]  // Условие с комбинированной арифметикой
        [DataRow("pow(sum[1, 2], 2) / 2 + sqrt(49)", new[] { 11.5 })]  // Вложения степеней и корней
        [DataRow("abs(-5) * mean[10, 20] + max(3, 7)", new[] { 82.0 })]  // Смешанные арифметические операции

        // Векторная арифметика
        [DataRow("[1, 2, 3] + [4, 5, 6] * 2", new[] { 9.0, 12.0, 15.0 })]  // Сложение и умножение векторов
        [DataRow("[10, 20] / [2, 4] + [3, 6]", new[] { 8.0, 11.0 })]  // Операции деления и сложения векторов
        [DataRow("[8, 6] - [3, 2] * 2", new[] { 2.0, 2.0 })]  // Разность и умножение
        [DataRow("[24, 25, 16, 8, 6] * [2, 5, 8, 8, 3]", new[] { 48.0, 125.0, 128.0, 64.0, 18.0 })]  // Умножение векторов

        // Более сложные комбинированные операции
        [DataRow("[1, 2, 3] + [4, 5, 6] + sqrt(16)", new[] { 9.0, 11.0, 13.0 })]  // Комбинированное сложение и квадратный корень
        [DataRow("sum[1, 2] * max([2, 5], [3, 4]) + mean[5, 6]", new[] { 14.5, 20.5 })]  // Умножение суммы и максимума, плюс среднее
        [DataRow("pow([1, 2], 2) + sum[3, 4, 5]", new[] { 13.0, 16.0 })]  // Степени и сумма векторов
        [DataRow("max([1, 4], [2, 3]) * sqrt(25) + min(1, 2)", new[] { 11.0, 21.0 })]  // Максимум, корень и минимум

        public void Calculate_AllCases_ReturnsCorrectResult(string input, object expected)
        {
            // Действие
            var result = _controller.Calculate(input, new Dictionary<string, double>(), new Dictionary<string, double[]>());

            // Проверка
            if (expected is double expectedScalar)
            {
                // Если ожидаем скаляр, приводим результат к массиву и проверяем первый элемент
                var resultScalar = (result as double[])?[0];

                // Проверка на null и сравнение с погрешностью
                Assert.IsNotNull(resultScalar, $"Результат не является скаляром. Ошибка в: {input}");
                Assert.AreEqual(expectedScalar, resultScalar.Value, 0.001, $"Ошибка в: {input}");
            }
            else if (expected is double[] expectedVector)
            {
                // Если ожидаем вектор, приводим результат к типу double[] и проверяем равенство с учетом погрешности
                var resultVector = result as double[];

                // Сравнение векторов с погрешностью
                for (int i = 0; i < expectedVector.Length; i++)
                {
                    if (resultVector != null)
                    {
                        Assert.AreEqual(expectedVector[i], resultVector[i], 0.001, $"Ошибка в: {input} | Индекс: {i}");
                    }
                }
            }
        }
    }
}
