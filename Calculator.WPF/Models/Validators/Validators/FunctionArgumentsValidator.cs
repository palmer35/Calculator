using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    /// <summary>
    /// Валидатор аргументов функций, запрещает использование '=' и '==' внутри функций.
    /// </summary>
    public class FunctionArgumentsValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckFunctionArguments(input);
        }

        private void CheckFunctionArguments(string input)
        {
            // Шаблон: имя функции, скобка (квадратная или круглая), содержимое, закрывающая скобка
            var functionPattern = @"\b(?<name>[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*)\s*(?<open>[\[\(])(?<args>[^\]\)]*)[\]\)]";

            foreach (Match match in Regex.Matches(input, functionPattern))
            {
                string functionName = match.Groups["name"].Value;
                string arguments = match.Groups["args"].Value;
                int argsOffsetInInput = match.Index + match.Groups["args"].Index;

                // Проверка только для зарезервированных функций (иначе игнорим пользовательские)
                if (ReservedFunctions.Contains(functionName))
                {
                    // Ищем одиночный '=' (не часть '>=', '<=', '!=') или двойной '=='
                    var invalidMatches = Regex.Matches(arguments, @"(?<![<>=!])=(?!=)|(?<!=)==(?!=)");

                    foreach (Match invalid in invalidMatches)
                    {
                        int errorPos = argsOffsetInInput + invalid.Index;

                        if (invalid.Value == "=")
                        {
                            ReportError(
                                "Знак '=' недопустим в аргументах функции. Используйте '<=', '>=', или '==' при необходимости логики.",
                                errorPos
                            );
                        }
                        else if (invalid.Value == "==")
                        {
                            ReportError(
                                "Оператор '==' недопустим в аргументах. Используйте '=' при передаче значений.",
                                errorPos
                            );
                        }
                    }
                }
            }
        }
    }
}
