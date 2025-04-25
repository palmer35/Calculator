using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    /// <summary>
    /// Валидатор проверяет, что между именем функции и скобкой не стоит пробел.
    /// </summary>
    public class FunctionSpacingValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForFunctionSpacing(input);
        }

        private void CheckForFunctionSpacing(string input)
        {
            string functionsPattern = string.Join("|", ReservedFunctions.Select(Regex.Escape));
            
            var pattern = $@"\b({functionsPattern})\s+\(";

            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                ReportError("Недопустимый пробел между именем функции и скобкой.", match.Index);
            }
        }
    }
}