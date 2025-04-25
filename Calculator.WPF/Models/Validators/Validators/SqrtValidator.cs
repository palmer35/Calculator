using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class SqrtValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForInvalidSqrtUsage(input);
        }

        /// <summary>
        /// Проверяет, используется ли sqrt с недопустимым аргументом (отрицательной переменной).
        /// </summary>
        private void CheckForInvalidSqrtUsage(string input)
        {
            var match = Regex.Match(input, @"sqrt\s*\(\s*-\s*\w+");
            if (match.Success)
            {
                ReportError("Недопустимый аргумент для sqrt: переменная не может быть отрицательной.", match.Index);
            }
        }
    }
}