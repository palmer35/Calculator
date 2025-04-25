using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class MissingOperandsValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForMissingOperands(input);
        }

        private void CheckForMissingOperands(string input)
        {
            var replacements = new Dictionary<string, string>();
            string tempInput = input;

            int funcCounter = 0;
            int strCounter = 0;

            // Заменяем строковые литералы: "..."
            tempInput = Regex.Replace(tempInput, @"""(?:[^""]|"""")*""", match =>
            {
                string key = $"#STR#{strCounter++}#";
                replacements[key] = match.Value;
                return key;
            });

            // Заменяем функции с квадратными скобками: sum[...], mean[...]
            tempInput = Regex.Replace(tempInput, @"(\w+)\[(?:[^\[\]]|(?<c>\[)|(?<-c>\]))+(?(c)(?!))\]", match =>
            {
                string functionName = match.Groups[1].Value;
                string key = $"{functionName}#FUNC#{funcCounter++}#";
                replacements[key] = match.Value;
                return key;
            });

            // Заменяем функции с круглыми скобками: (...)
            tempInput = Regex.Replace(tempInput, @"\([^()]*\)", match =>
            {
                string key = $"#FUNC#{funcCounter++}#";
                replacements[key] = match.Value;
                return key;
            });

            // Проверяем отсутствие операнда перед оператором (кроме унарного минуса)
            foreach (Match match in Regex.Matches(tempInput, @"(?<![0-9\]""#A-Z])\s*[+*/^](?![0-9(\[""#A-Z])"))
            {
                string operatorSymbol = match.Value.Trim();

                // Пропускаем унарный минус
                if (operatorSymbol == "-" &&
                    (match.Index == 0 || "+-*/^([, ".Contains(tempInput[match.Index - 1].ToString())))
                {
                    continue;
                }

                bool hasValidOperandAfter = false;
                int posAfterOperator = match.Index + match.Length;

                // Пропускаем пробелы после оператора
                while (posAfterOperator < tempInput.Length && char.IsWhiteSpace(tempInput[posAfterOperator]))
                {
                    posAfterOperator++;
                }

                // Проверка следующего символа после оператора
                if (posAfterOperator < tempInput.Length)
                {
                    char nextChar = tempInput[posAfterOperator];
                    hasValidOperandAfter =
                        char.IsLetterOrDigit(nextChar) ||
                        nextChar == '(' ||
                        nextChar == '[' ||
                        nextChar == '"' ||
                        nextChar == '#' || // маркеры #FUNC#, #STR#, #ARRAY#
                        nextChar == 'A' || 
                        nextChar == 'F';
                }

                if (!hasValidOperandAfter)
                {
                    ReportError($"Отсутствует операнд перед оператором '{operatorSymbol}'", match.Index);
                }
            }
        }
    }
}
