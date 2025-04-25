using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class NumbersValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForInvalidNumberSpacing(input);
        }

        private void CheckForInvalidNumberSpacing(string input)
        {
            // Ищем числа, разделённые пробелом (например: "123 456")
            var match = Regex.Match(input, @"\b\d+\s+\d+\b");

            if (match.Success)
            {
                ReportError("Число содержит пробел, что недопустимо.", match.Index);
            }
        }
    }
}