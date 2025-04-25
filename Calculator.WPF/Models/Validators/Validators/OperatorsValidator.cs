using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class OperatorsValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForInvalidOperators(input);
            CheckForDivisionByZero(input);
        }

        /// <summary>
        /// Проверяет выражение на недопустимые последовательности операторов,
        /// такие как: ++, --, **, ==, = (вне функций и выражений).
        /// </summary>
        private void CheckForInvalidOperators(string input)
        {
            // Проверка на повторяющиеся арифметические операторы (например: ++, **, //)
            var repeatedOperators = Regex.Match(input, @"([\+\-\*/^]{2,})");
            if (repeatedOperators.Success)
            {
                string operators = repeatedOperators.Groups[1].Value;
                ReportError($"Недопустимое сочетание операторов: {operators}.", repeatedOperators.Index);
            }

            // Проверка одиночного '=' и двойного '==' вне условий или функций
            var invalidEquals = Regex.Matches(input, @"(?<![<>=!])=(?!=)|(?<!=)==(?!=)");
            foreach (Match match in invalidEquals)
            {
                bool isInsideFunction = false;
                int bracketCount = 0;

                // Проверка на вложенность внутри скобок (функций)
                for (int i = 0; i < match.Index; i++)
                {
                    if (input[i] == '(' || input[i] == '[') bracketCount++;
                    else if (input[i] == ')' || input[i] == ']') bracketCount--;
                }

                // Если скобки не открыты — выражение вне функции
                if (bracketCount == 0)
                {
                    if (match.Value == "=")
                        ReportError("Знак '=' недопустим в выражении.", match.Index);
                    else
                        ReportError("Оператор '==' недопустим в выражении.", match.Index);
                }
            }
        }

        /// <summary>
        /// Проверяет, встречается ли деление на ноль.
        /// </summary>
        private void CheckForDivisionByZero(string input)
        {
            var divisionByZero = Regex.Match(input, @"(/|÷)\s*0\b");
            if (divisionByZero.Success)
            {
                ReportError("Деление на ноль недопустимо.", divisionByZero.Index);
            }
        }
    }
}
