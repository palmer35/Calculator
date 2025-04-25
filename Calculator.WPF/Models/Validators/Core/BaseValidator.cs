using Calculator.WPF.Models.Interfaces;
using ClassLibrary1;

namespace Calculator.WPF.Models.Validators.Core
{
    /// <summary>
    /// Базовый класс для валидаторов математических выражений.
    /// Содержит общие методы и набор зарезервированных функций.
    /// </summary>
    public abstract class BaseValidator : IValidator
    {
        /// <summary>
        /// Зарезервированные названия функций, которые нельзя использовать как переменные.
        /// </summary>
        protected static readonly HashSet<string> ReservedFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            "cos", "sin", "sqrt", "min", "max", "if", "pow", "abs", "mean", "sum"
        };

        /// <summary>
        /// Проверяет выражение на ошибки. Реализуется в наследниках.
        /// </summary>
        public abstract void Validate(string input);

        /// <summary>
        /// Генерирует исключение с указанием позиции ошибки.
        /// </summary>
        /// <param name="message">Текст ошибки</param>
        /// <param name="errorIndex">Индекс символа в строке (начиная с 0)</param>
        protected void ReportError(string message, int errorIndex)
        {
            throw new CalculatorException($"{message}\n(позиция {errorIndex + 1})", errorIndex + 1);
        }
    }
}