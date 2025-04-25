using System.Globalization;
using System.Windows.Controls;

namespace Calculator.WPF.Views
{
    /// <summary>
    /// Обрабатывает ввод переменных (скаляры и векторы) из UI.
    /// </summary>
    public class VariableInputProcessor
    {
        private readonly Dictionary<string, double[]> _vectorValues = new();

        /// <summary>
        /// Обрабатывает значения переменных, введённые пользователем.
        /// </summary>
        /// <param name="variables">Список переменных.</param>
        /// <param name="variablesWithPositions">Позиции переменных в выражении (для ошибок).</param>
        /// <param name="findTextBoxFunc">Функция для получения TextBox по имени переменной.</param>
        /// <returns>Словарь скалярных значений и словарь ошибок по переменным.</returns>
        public (Dictionary<string, double> Scalars, Dictionary<string, int> Errors) ProcessInputs(
            List<string> variables,
            Dictionary<string, (int Position, int Length)> variablesWithPositions,
            Func<string, TextBox?> findTextBoxFunc)
        {
            var scalarValues = new Dictionary<string, double>();
            var errorVariables = new Dictionary<string, int>();
            _vectorValues.Clear();

            foreach (var variable in variables)
            {
                var textBox = findTextBoxFunc(variable);
                if (textBox != null)
                {
                    ProcessSingleVariable(variable, textBox.Text, scalarValues, errorVariables, variablesWithPositions);
                }
            }

            return (scalarValues, errorVariables);
        }

        /// <summary>
        /// Возвращает обработанные векторы.
        /// </summary>
        public Dictionary<string, double[]> GetProcessedVectors() => _vectorValues;

        /// <summary>
        /// Обрабатывает ввод одной переменной.
        /// </summary>
        private void ProcessSingleVariable(
            string variable,
            string input,
            Dictionary<string, double> scalarValues,
            Dictionary<string, int> errorVariables,
            Dictionary<string, (int Position, int Length)> variablesWithPositions)
        {
            try
            {
                if (IsVectorInput(input))
                {
                    var vector = ParseVector(input);
                    _vectorValues[variable] = vector;
                }
                else if (TryParseScalar(input, out double scalar))
                {
                    scalarValues[variable] = scalar;
                }
                else
                {
                    AddError(variable, variablesWithPositions, errorVariables);
                }
            }
            catch
            {
                AddError(variable, variablesWithPositions, errorVariables);
            }
        }

        /// <summary>
        /// Проверяет, является ли ввод вектором.
        /// </summary>
        private bool IsVectorInput(string input) =>
            input.StartsWith("[") && input.EndsWith("]");

        /// <summary>
        /// Преобразует строку в массив чисел (вектор).
        /// </summary>
        private double[] ParseVector(string input) =>
            input.Trim('[', ']')
                 .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(s => double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture))
                 .ToArray();

        /// <summary>
        /// Пытается преобразовать строку в скалярное значение.
        /// </summary>
        private bool TryParseScalar(string input, out double value) =>
            double.TryParse(input.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Добавляет информацию об ошибке в словарь ошибок.
        /// </summary>
        private void AddError(string variable, Dictionary<string, (int Position, int Length)> positions, Dictionary<string, int> errors)
        {
            if (positions.TryGetValue(variable, out var posInfo))
            {
                errors[variable] = posInfo.Position;
            }
        }
    }
}
