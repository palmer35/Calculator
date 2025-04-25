namespace ClassLibrary1
{
    /// <summary>
    /// Исключение калькулятора, предназначенное для обработки ошибок парсинга и выполнения выражений.
    /// </summary>
    public class CalculatorException : Exception
    {
        public int ErrorPosition { get; }
        
        public Dictionary<string, int>? VariablePositions { get; }

        /// <summary>
        /// Создает исключение с сообщением и позицией ошибки.
        /// </summary>
        public CalculatorException(string message, int errorPosition = 0)
            : base(message)
        {
            ErrorPosition = errorPosition;
        }

        /// <summary>
        /// Создает исключение с сообщением и позициями переменных, если ошибка связана с переменными.
        /// </summary>
        public CalculatorException(string message, Dictionary<string, int>? variablePositions)
            : base(message)
        {
            VariablePositions = variablePositions;
            ErrorPosition = variablePositions?.Values.FirstOrDefault() ?? 0;
        }

        /// <summary>
        /// Переопределенный метод ToString для предоставления детализированного описания ошибки.
        /// </summary>
        public override string ToString()
        {
            var baseMessage = base.ToString();
            if (VariablePositions != null && VariablePositions.Any())
            {
                var positions = string.Join(", ", VariablePositions.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                return $"{baseMessage} (Positions: {positions})";
            }

            return $"{baseMessage} (Position: {ErrorPosition})";
        }
    }
}
