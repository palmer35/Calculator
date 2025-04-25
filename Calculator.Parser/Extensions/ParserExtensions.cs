using Calculator.Parser.Interfaces;

namespace Calculator.Parser.Extensions
{
    /// <summary>
    /// Расширения для интерфейса <see cref="IParser{TResult}"/>.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Выполняет парсинг входной строки и вычисляет результат.
        /// </summary>
        public static TResult Execute<TResult>(this IParser<TResult> parser, string input)
        {
            // Проверка на null для парсера
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser), "Парсер не может быть null.");
            }

            // Проверка на пустую или состоящую только из пробелов строку
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Входная строка не может быть пустой или состоять только из пробелов.", nameof(input));
            }

            try
            {
                // Выполняем парсинг входной строки в выражение
                var expression = parser.Parse(input);

                // Компилируем и выполняем выражение
                var result = expression.Compile().Invoke();

                // Преобразуем результат в тип TResult и возвращаем
                if (result is { } validResult)
                {
                    return validResult;
                }

                throw new InvalidOperationException($"Невозможно привести результат к типу {typeof(TResult)}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при выполнении парсинга: {ex.Message}", ex);
            }
        }
    }
}
