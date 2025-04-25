using Calculator.WPF.Models.Validators.Core;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    /// <summary>
    /// Проверяет математическое выражение на корректность скобок: (), [].
    /// </summary>
    public class BracketsValidator : BaseValidator
    {
        public override void Validate(string input)
        {
            CheckBalancedBrackets(input);
            CheckForStandaloneEmptyParentheses(input);
        }

        /// <summary>
        /// Проверяет сбалансированность скобок, учитывая вложенность и типы скобок.
        /// Игнорирует содержимое в кавычках.
        /// </summary>
        private void CheckBalancedBrackets(string input)
        {
            var stack = new Stack<(char BracketType, int Position)>();
            bool insideQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // Обработка входа/выхода из кавычек
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                    continue;
                }

                if (!insideQuotes)
                {
                    // Открывающая скобка
                    if (c == '(' || c == '[')
                    {
                        stack.Push((c, i));
                    }
                    // Закрывающая скобка
                    else if (c == ')' || c == ']')
                    {
                        if (stack.Count == 0)
                        {
                            ReportError($"Лишняя закрывающая скобка '{c}'", i);
                        }

                        var (openBracket, openPos) = stack.Pop();
                        if ((openBracket == '(' && c != ')') || (openBracket == '[' && c != ']'))
                        {
                            ReportError(
                                $"Несоответствие скобок: ожидалось '{(openBracket == '(' ? ')' : ']')}', но получено '{c}'",
                                openPos
                            );
                        }
                    }
                }
            }

            // Проверка на незакрытые скобки
            if (stack.Count > 0)
            {
                var (unclosedBracket, pos) = stack.Pop();
                ReportError($"Незакрытая скобка '{unclosedBracket}'", pos);
            }
        }

        /// <summary>
        /// Проверяет, не содержится ли в выражении пустых круглых скобок: ()
        /// </summary>
        private void CheckForStandaloneEmptyParentheses(string input)
        {
            var match = Regex.Match(input, @"\(\s*\)");
            if (match.Success)
            {
                ReportError("Пустые скобки '()' недопустимы", match.Index);
            }
        }
    }
}
