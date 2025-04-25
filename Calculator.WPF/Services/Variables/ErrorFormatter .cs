namespace Calculator.WPF.Services.Variables
{
    /// <summary>
    /// Форматирует сообщения об ошибках, связанных с переменными.
    /// </summary>
    public class ErrorFormatter
    {
        /// <summary>
        /// Создаёт текст ошибки на основе словаря переменных с их позициями.
        /// </summary>
        /// <param name="errorVariables">Словарь переменных и позиций, где обнаружена ошибка.</param>
        /// <returns>Отформатированная строка с описанием ошибок.</returns>
        public string FormatVariableError(Dictionary<string, int>? errorVariables)
        {
            if (errorVariables == null || errorVariables.Count == 0)
                return "Неизвестная ошибка в переменных.";

            return "Ошибка в значениях переменных:\n" +
                   string.Join("\n", errorVariables.Select(kvp =>
                       $"• {CleanVariableName(kvp.Key)} (позиция {kvp.Value})"));
        }

        /// <summary>
        /// Убирает кавычки вокруг имени переменной, если они есть.
        /// </summary>
        private string CleanVariableName(string name) =>
            name.StartsWith("\"") && name.EndsWith("\"")
                ? name.Trim('"')
                : name;
    }
}