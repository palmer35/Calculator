using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class QuotesValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckForUnclosedQuotes(input);
            CheckForEmptyQuotes(input);
        }

        /// <summary>
        /// Проверяет наличие незакрытых кавычек в выражении.
        /// </summary>
        private void CheckForUnclosedQuotes(string input)
        {
            bool insideQuotes = false;
            int? firstQuotePos = null;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"')
                {
                    if (!insideQuotes)
                    {
                        firstQuotePos = i;
                    }
                    else
                    {
                        firstQuotePos = null;
                    }

                    insideQuotes = !insideQuotes;
                }
            }

            if (firstQuotePos.HasValue)
            {
                ReportError("Незакрытая кавычка", firstQuotePos.Value);
            }
        }

        /// <summary>
        /// Проверяет наличие пустых кавычек ("").
        /// </summary>
        private void CheckForEmptyQuotes(string input)
        {
            var match = Regex.Match(input, @"""""");
            if (match.Success)
            {
                ReportError("Пустые кавычки недопустимы", match.Index);
            }
        }
    }
}