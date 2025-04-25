using Calculator.Parser.Calculator;
using Calculator.WPF.Models.Extractors;
using Calculator.WPF.Models.Substitutors;
using Calculator.WPF.Models.Validators;
using ClassLibrary1;

namespace Calculator.WPF.Services.Calculation
{
    /// <summary>
    /// Контроллер для валидации, подстановки и вычисления математических выражений.
    /// </summary>
    public class CalculationController
    {
        private readonly ExpressionValidator _validator;
        private readonly VariableExtractor _extractor;
        private readonly VariableSubstitutor _substitutor;
        private readonly ExpressionParser _parser;

        /// <summary>
        /// Инициализирует зависимости контроллера.
        /// </summary>
        public CalculationController(
            ExpressionValidator validator,
            VariableExtractor extractor,
            VariableSubstitutor substitutor,
            ExpressionParser parser)
        {
            _validator = validator;
            _extractor = extractor;
            _substitutor = substitutor;
            _parser = parser;
        }

        /// <summary>
        /// Извлекает список переменных из выражения.
        /// </summary>
        public List<string> GetVariables(string input) =>
            _extractor.ExtractVariables(input);

        /// <summary>
        /// Извлекает переменные и их позиции в выражении.
        /// </summary>
        public Dictionary<string, (int Position, int Length)> GetVariablesWithPositions(string input) =>
            _extractor.ExtractVariablesWithPositions(input);

        /// <summary>
        /// Выполняет валидацию математического выражения.
        /// </summary>
        public void ValidateExpression(string input)
        {
            _validator.Validate(input);
        }

        /// <summary>
        /// Подставляет переменные и вычисляет результат выражения.
        /// </summary>
        /// <param name="input">Исходное выражение.</param>
        /// <param name="scalarValues">Скалярные значения переменных.</param>
        /// <param name="vectorValues">Векторные значения переменных.</param>
        /// <returns>Результат вычисления — скаляр или вектор.</returns>
        public object Calculate(
            string input,
            Dictionary<string, double> scalarValues,
            Dictionary<string, double[]> vectorValues)
        {
            try
            {
                // Шаг 1: подстановка значений переменных
                string substituted = _substitutor.Substitute(input, scalarValues, vectorValues);

                // Шаг 2: валидация выражения после подстановки
                ValidateExpression(substituted);

                // Шаг 3: парсинг и выполнение выражения
                var expression = _parser.Parse(substituted);
                var rawResult = expression.Compile().DynamicInvoke();

                // Шаг 4: обработка результата
                return rawResult switch
                {
                    double[] vector => vector,
                    double scalar => new[] { scalar }, // Преобразуем скаляр к вектору для унификации
                    _ => throw new CalculatorException("Неверный тип результата.")
                };
            }
            catch (CalculatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CalculatorException($"Ошибка: {ex.Message}", 0);
            }
        }
    }
}
