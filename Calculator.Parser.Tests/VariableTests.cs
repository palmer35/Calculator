using Calculator.WPF.Models.Extractors;
using Calculator.WPF.Models.Validators;
using ClassLibrary1;

namespace Calculator.Parser.Tests.Tests
{
    [TestClass]
    public class VariableTests
    {
        private VariableExtractor _variableExtractor;
        private ExpressionValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _variableExtractor = new VariableExtractor();
            _validator = new ExpressionValidator();
        }

        // Тесты на корректные переменные
        [DataTestMethod]
        [DataRow("\"dqff\" + 10", new[] { "dqff" }, DisplayName = "QuotedVariable_ExtractedCorrectly")]
        [DataRow("аца_2 + 5", new[] { "аца_2" }, DisplayName = "UnicodeVariable_ExtractedCorrectly")]
        [DataRow("\"dqff\" + аца_2 - 3", new[] { "dqff", "аца_2" }, DisplayName = "MixedVariables_ExtractedCorrectly")]
        [DataRow("a + b + c", new[] { "a", "b", "c" }, DisplayName = "MultipleVariables_ExtractedCorrectly")]
        [DataRow("x1 + 10", new[] { "x1" }, DisplayName = "NumberedVariable_ExtractedCorrectly")]
        [DataRow("_var + 5", new[] { "_var" }, DisplayName = "UnderscoreVariable_ExtractedCorrectly")]
        [DataRow("sqrt(x) + sin(y)", new[] { "x", "y" }, DisplayName = "FunctionVariables_ExtractedCorrectly")]
        public void ValidVariables_ExtractedSuccessfully(string input, string[] expectedVariables)
        {
            // Act
            var variables = _variableExtractor.ExtractVariables(input);

            // Assert
            CollectionAssert.AreEquivalent(expectedVariables, variables.ToList(),
                $"Ожидались переменные: {string.Join(", ", expectedVariables)}");

            // Дополнительная проверка валидации
            ValidateWithoutError(input);
        }

        // Тесты на некорректные переменные
        [DataTestMethod]
        [DataRow("my var + 5", "my var",
            "Недопустимый пробел между идентификаторами: \"my var\"", 3,
            DisplayName = "SpaceInVariableName_ThrowsException")]
        [DataRow("var$ + 10", "var$",
            "Недопустимый символ: '$'", 4,
            DisplayName = "SpecialCharInVariable_ThrowsException")]
        [DataRow("\"unclosed", "\"unclosed",
            "Незакрытая кавычка", 1,
            DisplayName = "UnclosedQuote_ThrowsException")]
        public void InvalidVariables_ThrowsDetailedException(
            string input,
            string errorPart,
            string expectedError,
            int expectedPosition)
        {
            // Act & Assert
            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _validator.Validate(input));

            // Проверка сообщения
            StringAssert.Contains(ex.Message, expectedError,
                $"Некорректное сообщение об ошибке: {ex.Message}");

            // Проверка позиции
            Assert.AreEqual(expectedPosition, ex.ErrorPosition,
                $"Некорректная позиция ошибки. Ожидалось: {expectedPosition}, получено: {ex.ErrorPosition}");

            // Проверка наличия проблемной части
            StringAssert.Contains(input, errorPart,
                "Ошибка не связана с ожидаемой частью выражения");
        }

        // Дополнительная проверка на успешную валидацию
        private void ValidateWithoutError(string input)
        {
            try
            {
                _validator.Validate(input);
            }
            catch (CalculatorException ex)
            {
                Assert.Fail($"Неожиданная ошибка валидации: {ex.Message}\nВ выражении: {input}");
            }
        }

        [TestMethod]
        public void VariableWithMaxLength_Accepted()
        {
            // Arrange
            var longVar = new string('a', 255);
            var input = $"{longVar} + 5";

            // Act
            var variables = _variableExtractor.ExtractVariables(input);

            // Assert
            CollectionAssert.Contains(variables.ToList(), longVar);
            ValidateWithoutError(input);
        }


        [DataTestMethod]
        [DataRow("sum(\"var1\", var2)", "sum(",
        "Функция 'sum' должна использовать квадратные скобки []", 1,
        DisplayName = "WrongBracketsInFunction_ThrowsException")]
        public void WrongBracketsInFunction_ThrowsException(string input, string errorPart, string expectedError, int expectedPosition)
        {
            // Act & Assert
            var ex = Assert.ThrowsException<CalculatorException>(() =>
                _validator.Validate(input));

            // Проверка сообщения
            StringAssert.Contains(ex.Message, expectedError,
                $"Некорректное сообщение об ошибке: {ex.Message}");

            // Проверка позиции
            Assert.AreEqual(expectedPosition, ex.ErrorPosition,
                $"Некорректная позиция ошибки. Ожидалось: {expectedPosition}, получено: {ex.ErrorPosition}");

            // Проверка наличия проблемной части
            StringAssert.Contains(input, errorPart,
                "Ошибка не связана с ожидаемой частью выражения");
        }

    }
}
