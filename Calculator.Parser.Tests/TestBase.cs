using Calculator.Parser.Calculator;
using Calculator.WPF.Models.Extractors;
using Calculator.WPF.Models.Substitutors;
using Calculator.WPF.Models.Validators;
using Calculator.WPF.Services.Calculation;

namespace Calculator.Parser.Tests
{
    public abstract class TestBase
    {
        /// <summary>
        /// Создаёт экземпляр контроллера для выполнения вычислений.
        /// </summary>
        protected CalculationController CreateProcessor()
        {
            // Создаем экземпляры всех зависимостей
            var expressionParser = new ExpressionParser();  // Создаем объект ExpressionParser
            return new CalculationController(
                new ExpressionValidator(),
                new VariableExtractor(),
                new VariableSubstitutor(),
                expressionParser  // Передаем его в конструктор
            );
        }

        /// <summary>
        /// Создаёт словарь векторных переменных с произвольными значениями.
        /// </summary>
        protected Dictionary<string, double[]?> CreateVectorVariables(
            double[]? a = null,
            double[]? b = null,
            double[]? c = null)
        {
            return new Dictionary<string, double[]?>
            {
                { "a", a },
                { "b", b },
                { "c", c }
            }.Where(pair => pair.Value != null)
             .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Создаёт пустой словарь скалярных переменных.
        /// </summary>
        protected Dictionary<string, double> CreateEmptyScalarVariables()
        {
            return new Dictionary<string, double>();
        }
    }
}
