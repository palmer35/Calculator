using Calculator.WPF.Models.Interfaces;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Extractors;

/// <summary>
/// Извлекает переменные из математических выражений.
/// Поддерживает как обычные переменные, так и заключённые в кавычки.
/// </summary>
public class VariableExtractor : IVariableExtractor, IVariablePositionExtractor
{
    // Зарезервированные слова, которые НЕ считаются переменными
    private static readonly HashSet<string> _reservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
    "cos", "sin", "sqrt", "pi", "e", "min", "max", "if", "pow", "abs", "Mean", "Sum", "Fact", "log"
    };


    // Регулярные выражения
    private const string QuotedPattern = @"""(?<content>(?:\\""|[^""])*)""";
    private const string UnquotedPattern = @"\b(?<var>[a-zA-Zа-яА-ЯёЁ_][\wа-яА-ЯёЁ]*|\d+[a-zA-Zа-яА-ЯёЁ_][\wа-яА-ЯёЁ]*)\b";

    /// <summary>
    /// Извлекает все уникальные переменные из выражения.
    /// </summary>
    public List<string> ExtractVariables(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ExtractQuotedVariables(input, variables);
        ExtractUnquotedVariables(input, variables);

        return variables.ToList();
    }

    /// <summary>
    /// Извлекает переменные с их позициями в исходной строке.
    /// </summary>
    public Dictionary<string, (int Position, int Length)> ExtractVariablesWithPositions(string input)
    {
        var result = new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(input))
            return result;

        ExtractQuotedVariablesWithPositions(input, result);
        ExtractUnquotedVariablesWithPositions(input, result);

        return result;
    }

    /// <summary>
    /// Извлекает переменные, заключённые в кавычки (например, "varName").
    /// </summary>
    private void ExtractQuotedVariables(string input, HashSet<string> variables)
    {
        foreach (Match match in Regex.Matches(input, QuotedPattern))
        {
            var content = match.Groups["content"].Value;
            if (!string.IsNullOrEmpty(content))
                variables.Add(content);
        }
    }

    /// <summary>
    /// Извлекает переменные без кавычек (например, alpha, temp1 и т.д.).
    /// </summary>
    private void ExtractUnquotedVariables(string input, HashSet<string> variables)
    {
        // Исключаем quoted-секции из строки, чтобы не дублировать переменные
        var filteredInput = Regex.Replace(input, QuotedPattern, "");

        foreach (Match match in Regex.Matches(filteredInput, UnquotedPattern))
        {
            var varName = match.Groups["var"].Value;
            if (IsValidVariable(varName))
                variables.Add(varName);
        }
    }

    /// <summary>
    /// Извлекает quoted-переменные и их позиции в строке (с обрезанными кавычками).
    /// </summary>
    private void ExtractQuotedVariablesWithPositions(string input,
        Dictionary<string, (int Position, int Length)> result)
    {
        foreach (Match match in Regex.Matches(input, QuotedPattern))
        {
            var content = match.Groups["content"].Value;
            if (!string.IsNullOrEmpty(content) && !result.ContainsKey(content))
            {
                // Позиция без кавычек, поэтому +1 и длина -2
                result[content] = (match.Index + 1, match.Length - 2);
            }
        }
    }

    /// <summary>
    /// Извлекает unquoted-переменные и их позиции.
    /// </summary>
    private void ExtractUnquotedVariablesWithPositions(string input,
        Dictionary<string, (int Position, int Length)> result)
    {
        // Заменяем quoted-секции на пробелы, чтобы сохранить индексацию
        var filteredInput = Regex.Replace(input, QuotedPattern, m => new string(' ', m.Length));

        foreach (Match match in Regex.Matches(filteredInput, UnquotedPattern))
        {
            var varName = match.Groups["var"].Value;
            if (IsValidVariable(varName) && !result.ContainsKey(varName))
                result[varName] = (match.Index, match.Length);
        }
    }

    /// <summary>
    /// Проверяет, является ли строка допустимой переменной (не число и не ключевое слово).
    /// </summary>
    private bool IsValidVariable(string name) =>
        !_reservedWords.Contains(name) && !double.TryParse(name, out _);
}
