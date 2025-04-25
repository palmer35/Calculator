using Calculator.WPF.Models.Validators.Core;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Validators.Validators
{
    public class FunctionsValidator : BaseValidator
    {
        private static readonly Dictionary<string, int> FunctionArgumentCounts = new(StringComparer.OrdinalIgnoreCase)
        {
            { "cos", 1 }, { "sin", 1 }, { "sqrt", 1 }, { "abs", 1 },
            { "pow", 2 }, { "min", 2 }, { "max", 2 }, { "if", 3 },
            { "log", 2},
            { "mean", -1 }, { "sum", -1 },  { "fact", -1 }
        };

        private static readonly HashSet<string> SquareBracketFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            "sum", "mean", "fact"
        };

        private static new readonly HashSet<string> ReservedFunctions = new(FunctionArgumentCounts.Keys, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Главный метод валидации — запускает серию проверок.
        /// </summary>
        public override void Validate(string input)
        {
            CheckForInvalidFunctions(input);
            CheckBracketTypes(input);
            CheckFunctionArgumentsCount(input);
            CheckForIncorrectFunctionUsage(input);
            CheckVectorFunctionArguments(input);
        }

        /// <summary>
        /// Проверяет корректность аргументов векторных функций (sum, mean, min, max, pow).
        /// </summary>
        private void CheckVectorFunctionArguments(string input)
        {
            var pattern = @"\b(sum|mean|fact)\s*\[([^\]]+)\]";
            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.IgnoreCase))
            {
                var function = match.Groups[1].Value.ToLower();
                var args = ParseArguments(match.Groups[2].Value.Trim());
                CheckSumMeanArguments(function, args, match);
            }

            foreach (var (func, index) in FindFunctionCalls(input, new[] { "min", "max", "pow", "log"}))
            {
                var args = ParseArguments(ExtractArgumentsFromPosition(input, index + func.Length));
                if (args.Length != 2)
                    ReportError($"Функция '{func}' требует ровно 2 аргумента", index);
                else
                    CheckElementwiseFunctionArguments(func, args, Match.Empty);
            }
        }

        /// <summary>
        /// Ищет вызовы указанных функций в тексте.
        /// </summary>
        private IEnumerable<(string functionName, int index)> FindFunctionCalls(string input, string[] functionNames)
        {
            foreach (var func in functionNames)
            {
                var regex = new Regex(@"\b" + func + @"\s*\(", RegexOptions.IgnoreCase);
                foreach (Match match in regex.Matches(input))
                    yield return (func.ToLower(), match.Index);
            }
        }

        /// <summary>
        /// Извлекает строку аргументов, начиная с позиции после имени функции.
        /// </summary>
        private string ExtractArgumentsFromPosition(string input, int startIndex)
        {
            int parenStart = input.IndexOf('(', startIndex);
            if (parenStart == -1) return string.Empty;

            int level = 0;
            var builder = new StringBuilder();
            for (int i = parenStart + 1; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '(') level++;
                if (c == ')')
                {
                    if (level == 0) return builder.ToString();
                    level--;
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Проверка аргументов функций sum и mean.
        /// </summary>
        private void CheckSumMeanArguments(string function, string[] args, Match match)
        {
            bool hasVectorVar = args.Any(IsVectorVariable);
            bool hasScalars = args.Any(arg => IsNumber(arg) || IsVectorLiteral(arg));

            if (hasVectorVar && hasScalars)
                ReportError($"Функция '{function}' должна принимать либо только векторную переменную, либо только числа/литералы", match.Index);

            if (args.Count(IsVectorVariable) > 1)
                ReportError($"Функция '{function}' может принимать только одну векторную переменную", match.Index);
        }

        /// <summary>
        /// Проверка на допустимость аргументов в векторных функциях (pow, min, max).
        /// </summary>
        private void CheckElementwiseFunctionArguments(string function, string[] args, Match match)
        {
            if (args.Count(IsVectorVariable) > 0 &&
                args.Any(arg => !IsVectorVariable(arg) && !IsNumber(arg) && !IsVectorLiteral(arg)))
            {
                ReportError($"Функция '{function}' не поддерживает смешивание векторных переменных с выражениями", match.Index);
            }
            
            if (function.Equals("pow", StringComparison.OrdinalIgnoreCase) && args.Length == 2)
            {
                if (IsNumber(args[1]))
                {
                    if (double.TryParse(args[1], out double exponent) && exponent < 0)
                    {
                        ReportError($"Второй аргумент функции 'pow' не может быть отрицательным", match.Index);
                    }
                }
                else if (IsVectorLiteral(args[1]))
                {
                    var values = args[1].Trim('[', ']').Split(',')
                        .Select(x => double.Parse(x.Trim(), CultureInfo.InvariantCulture));
                    if (values.Any(v => v < 0))
                    {
                        ReportError($"Второй аргумент функции 'pow' не может содержать отрицательные значения", match.Index);
                    }
                }
            }
        }

        /// <summary>
        /// Проверка количества аргументов в каждой функции.
        /// </summary>
        private void CheckFunctionArgumentsCount(string input)
        {
            foreach (var (name, _, argsString, index) in ParseFunctions(input))
            {
                if (!FunctionArgumentCounts.TryGetValue(name, out var expected)) continue;

                var args = ParseArguments(argsString);
                if (expected == -1 && args.Length == 0)
                    ReportError($"Функция '{name}' должна принимать хотя бы один аргумент.", index);
                else if (expected != -1 && args.Length != expected)
                    ReportError($"Функция '{name}' должна принимать {expected} аргумента(ов), передано: {args.Length}.", index);

                if (name.Equals("if", StringComparison.OrdinalIgnoreCase) && args.Length == 3 &&
                    !bool.TryParse(args[0], out _) &&
                    !Regex.IsMatch(args[0], @"[<>!=]=?|&&|\|\|"))
                {
                    ReportError($"Первый аргумент функции 'if' должен быть булевым выражением, получено: '{args[0]}'.", index);
                }
            }
        }

        /// <summary>
        /// Парсит аргументы из строки, корректно обрабатывая вложенные скобки.
        /// </summary>
        private string[] ParseArguments(string arguments)
        {
            int level = 0;
            var result = new List<string>();
            var current = new StringBuilder();

            foreach (char c in arguments)
            {
                if (c is '(' or '[') level++;
                else if (c is ')' or ']') level--;
                if (c == ',' && level == 0)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                    current.Append(c);
            }

            if (current.Length > 0)
                result.Add(current.ToString().Trim());

            return result.ToArray();
        }

        /// <summary>
        /// Извлекает информацию о функциях: имя, тип скобок, аргументы.
        /// </summary>
        private IEnumerable<(string FunctionName, string BracketType, string Arguments, int Index)> ParseFunctions(string input)
        {
            var regex = new Regex(@"\b([a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*)\s*([\[\(])", RegexOptions.IgnoreCase);

            foreach (Match match in regex.Matches(input))
            {
                string name = match.Groups[1].Value;
                string bracket = match.Groups[2].Value;
                char open = bracket == "(" ? '(' : '[';
                char close = bracket == "(" ? ')' : ']';

                int index = match.Index + match.Length;
                int depth = 1;
                var argsBuilder = new StringBuilder();

                while (index < input.Length && depth > 0)
                {
                    char c = input[index];
                    if (c == open) depth++;
                    else if (c == close) depth--;
                    if (depth > 0) argsBuilder.Append(c);
                    index++;
                }

                if (depth == 0)
                    yield return (name, bracket, argsBuilder.ToString().Trim(), match.Index);
            }
        }

        /// <summary>
        /// Проверка на неизвестные функции.
        /// </summary>
        private void CheckForInvalidFunctions(string input)
        {
            var regex = new Regex(@"\b([a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*)\s*[\[\(]");
            foreach (Match match in regex.Matches(input))
            {
                if (!ReservedFunctions.Contains(match.Groups[1].Value))
                    ReportError($"Неизвестная функция: {match.Groups[1].Value}", match.Index);
            }
        }

        /// <summary>
        /// Проверка правильного типа скобок у функций.
        /// </summary>
        private void CheckBracketTypes(string input)
        {
            var regex = new Regex(@"\b([a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*)\s*(\[|\()");
            foreach (Match match in regex.Matches(input))
            {
                var name = match.Groups[1].Value;
                var bracket = match.Groups[2].Value;

                if (bracket == "[" && !SquareBracketFunctions.Contains(name))
                    ReportError($"Функция '{name}' должна использовать круглые скобки ()", match.Index);
                else if (bracket == "(" && SquareBracketFunctions.Contains(name))
                    ReportError($"Функция '{name}' должна использовать квадратные скобки []", match.Index);
            }
        }

        /// <summary>
        /// Проверяет корректный синтаксис вызова функций.
        /// </summary>
        private void CheckForIncorrectFunctionUsage(string input)
        {
            foreach (Match match in Regex.Matches(input, @"\b(Mean|Sum|Fact)\b(?!\s*\[)", RegexOptions.IgnoreCase))
                ReportError($"Функция '{match.Value}' должна вызываться с квадратными скобками: {match.Value}[]", match.Index);

            foreach (Match match in Regex.Matches(input, @"\b(Cos|Sin|Sqrt|Min|Max|If|Pow|Abs|Log)\b(?!\s*\()", RegexOptions.IgnoreCase))
                ReportError($"Функция '{match.Value}' должна вызываться с круглыми скобками: {match.Value}()", match.Index);
        }

        /// <summary>
        /// Проверка: аргумент — векторная переменная.
        /// </summary>
        private bool IsVectorVariable(string arg) =>
            !arg.StartsWith("[") && !arg.EndsWith("]") && !double.TryParse(arg, out _);

        /// <summary>
        /// Проверка: аргумент — векторный литерал.
        /// </summary>
        private bool IsVectorLiteral(string arg) =>
            arg.StartsWith("[") && arg.EndsWith("]");

        /// <summary>
        /// Проверка: аргумент — число.
        /// </summary>
        private bool IsNumber(string arg) =>
            double.TryParse(arg, out _);
    }
}
