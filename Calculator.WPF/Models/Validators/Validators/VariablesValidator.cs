using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class VariablesValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForInvalidVariableSpacing(input);
            CheckForInvalidCharacters(input);
        }

        /// <summary>
        /// Проверяет наличие недопустимых пробелов между идентификаторами переменных.
        /// Игнорирует пробелы внутри строк (в кавычках) и между зарезервированными функциями.
        /// </summary>
        private void CheckForInvalidVariableSpacing(string input)
        {
            // Заменяем содержимое строк, чтобы избежать ложных срабатываний
            string sanitizedInput = Regex.Replace(input, @"""[^""]*""", "\"QUOTED\"");

            var matches = Regex.Matches(sanitizedInput, @"(\w+)\s+(\w+)");

            foreach (Match match in matches)
            {
                string left = match.Groups[1].Value;
                string right = match.Groups[2].Value;

                // Пропускаем, если одна из сторон — строка
                if (left == "QUOTED" || right == "QUOTED")
                    continue;

                // Пропускаем, если обе стороны — допустимые функции
                if (!ReservedFunctions.Contains(left) && !ReservedFunctions.Contains(right))
                {
                    int spaceIndex = match.Index + left.Length;
                    ReportError(
                        $"Недопустимый пробел между идентификаторами: \"{left} {right}\"",
                        spaceIndex
                    );
                }
            }
        }

        /// <summary>
        /// Проверяет наличие недопустимых символов вне строк.
        /// </summary>
        private void CheckForInvalidCharacters(string input)
        {
            // Исключаем строки из проверки
            string sanitizedInput = Regex.Replace(input, @"""[^""]*""", "\"QUOTED\"");

            // Добавлен символ % в список разрешённых
            var match = Regex.Match(sanitizedInput, @"[^\wа-яА-Я+\-*/%^().,\[\] _<>=!\""]");
            if (match.Success)
            {
                ReportError($"Недопустимый символ: '{match.Value}'", match.Index);
            }
        }
    }
}
