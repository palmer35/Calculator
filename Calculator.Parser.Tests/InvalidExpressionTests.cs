using Calculator.WPF.Services.Calculation;
using ClassLibrary1;

namespace Calculator.Parser.Tests.Tests
{
    [TestClass]
    public class CalculationControllerTests : TestBase
    {
        private CalculationController _controller;

        [TestInitialize]
        public void Setup() => _controller = CreateProcessor();

        // Тесты на некорректные функции
        [DataTestMethod]
        [DataRow("sum[]", "Функция 'sum' должна принимать хотя бы один аргумент", 1)]
        [DataRow("mean[]", "Функция 'mean' должна принимать хотя бы один аргумент", 1)]
        [DataRow("cos()", "Пустые скобки '()' недопустимы", 4)]
        [DataRow("pow(1)", "Функция 'pow' должна принимать 2 аргумента(ов), передано: 1", 1)]
        [DataRow("if(1, 2)", "Функция 'if' должна принимать 3 аргумента(ов), передано: 2", 1)]
        [DataRow("unknown()", "Пустые скобки '()' недопустимы", 8)]
        public void Calculate_InvalidFunctions_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double>();
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }

        // Тесты на синтаксические ошибки
        [DataTestMethod]
        [DataRow("sum(1, 2", "Незакрытая скобка '('", 4)]
        [DataRow("max(1,,2)", "Не может быть двух запятых подряд", 6)]
        [DataRow("a + '5'", "Недопустимый символ: '''", 5)]
        [DataRow("sum(1,)", "Запятая не может быть последним символом", 6)]
        [DataRow("1,,2", "Недопустимое использование запятой", 2)]
        [DataRow("2 , 3", "Недопустимое использование запятой вне скобок", 3)]
        public void Calculate_SyntaxErrors_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double> { ["a"] = 2 };
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }

        // Тесты на математические ошибки
        [DataTestMethod]
        [DataRow("sqrt(-1)", "Недопустимый аргумент для sqrt: переменная не может быть отрицательной.", 1)]
        [DataRow("1 / 0", "Деление на ноль недопустимо", 3)]
        [DataRow("pow(0, -1)", "Второй аргумент функции 'pow' не может быть отрицательным", 1)]
        public void Calculate_MathErrors_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double>();
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }

        // Тесты на ошибку с неопустимыми символами
        [DataTestMethod]
        [DataRow("a + #", "Недопустимый символ: '#'", 5)]
        [DataRow("sum(1, 2)", "Функция 'sum' должна использовать квадратные скобки []", 1)]
        [DataRow("cos(@)", "Недопустимый символ: '@'", 5)]
        [DataRow("sqrt($)", "Недопустимый символ: '$'", 6)]
        public void Calculate_InvalidCharacters_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double> { ["a"] = 1 };
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }

        // Тесты на незакрытые скобки
        [DataTestMethod]
        [DataRow("sum(1, 2", "Незакрытая скобка '('", 4)]
        [DataRow("sqrt(1 + (2", "Незакрытая скобка '('", 10)]
        [DataRow("pow(2, 3", "Незакрытая скобка '('", 4)]
        public void Calculate_UnclosedParenthesis_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double>();
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }

        // Тесты на лишние символы, такие как дополнительные запятые и числа
        [DataTestMethod]
        [DataRow("sum[1, , 2]", "Функция 'sum' должна принимать либо только векторную переменную, либо только числа/литералы", 1)]
        [DataRow("pow(2,,3)", "Не может быть двух запятых подряд", 6)]
        [DataRow("max(2,,3)", "Не может быть двух запятых подряд", 6)]
        [DataRow("pow(1, 2, 3)", "Функция 'pow' должна принимать 2 аргумента(ов), передано: 3", 1)]
        [DataRow("sqrt(1, 2)", "Функция 'sqrt' должна принимать 1 аргумента(ов), передано: 2.", 1)]
        public void Calculate_ExcessiveArguments_ThrowsException(string input, string expectedError, int expectedErrorPosition)
        {
            var scalars = new Dictionary<string, double>();
            var vectors = new Dictionary<string, double[]>();

            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _controller.Calculate(input, scalars, vectors));

            StringAssert.Contains(ex.Message, expectedError);
            Assert.AreEqual(expectedErrorPosition, ex.ErrorPosition);
        }
    }
}
