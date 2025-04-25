using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    /// <summary>
    /// Валидатор, проверяющий корректность использования запятых в выражении.
    /// </summary>
    public class CommasValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForInvalidCommas(input);
        }

        private void CheckForInvalidCommas(string input)
        {
            bool insideQuotes = false;
            int bracketLevel = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"') insideQuotes = !insideQuotes;

                if (!insideQuotes)
                {
                    if (c == '(' || c == '[') bracketLevel++;
                    else if (c == ')' || c == ']') bracketLevel--;

                    if (c == ',' && bracketLevel == 0)
                    {
                        bool isInvalid = true;
                        if (i > 0)
                        {
                            char prev = input[i - 1];
                            isInvalid = !(char.IsDigit(prev) || prev == ')' || prev == ']' || prev == '"');
                        }

                        if (i < input.Length - 1)
                        {
                            char next = input[i + 1];
                            isInvalid = isInvalid || !(char.IsDigit(next) || next == '(' || next == '[' || next == '"');
                        }

                        if (isInvalid)
                        {
                            ReportError("Недопустимое использование запятой вне скобок", i);
                        }
                    }
                }
            }
            
            // Проверка аргументов функций: Sum[1, 2], Mean(1, 2), etc.
            var functionPattern = @"\b([a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*)\s*[\[\(]([^)\]]*)[\]\)]";
            foreach (Match match in Regex.Matches(input, functionPattern))
            {
                string arguments = match.Groups[2].Value;
                int startPos = match.Index + match.Groups[2].Index;

                if (arguments.StartsWith(","))
                {
                    ReportError("Запятая не может быть первым символом в аргументах функции", startPos);
                }

                if (arguments.EndsWith(","))
                {
                    ReportError("Запятая не может быть последним символом в аргументах функции", startPos + arguments.Length - 1);
                }

                for (int i = 0; i < arguments.Length - 1; i++)
                {
                    if (arguments[i] == ',' && arguments[i + 1] == ',')
                    {
                        ReportError("Не может быть двух запятых подряд", startPos + i);
                    }
                }
            }
        }
    }
}