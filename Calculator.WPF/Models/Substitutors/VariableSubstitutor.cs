using Calculator.WPF.Models.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Calculator.WPF.Models.Substitutors
{
    /// <summary>
    /// Подставляет значения переменных (скалярных и векторных) в выражения.
    /// </summary>
    public class VariableSubstitutor : IVariableSubstitutor
    {
        public string Substitute(
            string input,
            Dictionary<string, double> scalarVariables,
            Dictionary<string, double[]> vectorVariables)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = ReplaceQuotedVariables(input, scalarVariables);
            input = ProcessVectorOperations(input, vectorVariables);
            input = ValidateVectorArgumentsInFunctions(input, vectorVariables); // ← новое
            input = ReplaceRegularVariables(input, scalarVariables, vectorVariables);

            return input;
        }

        private string ReplaceQuotedVariables(string input, Dictionary<string, double> variables)
        {
            return Regex.Replace(input, @"""([^""]+)""", match =>
            {
                var key = match.Groups[1].Value;
                return variables.TryGetValue(key, out double val)
                    ? val.ToString(CultureInfo.InvariantCulture)
                    : match.Value;
            }, RegexOptions.IgnoreCase);
        }

        private string ProcessVectorOperations(string input, Dictionary<string, double[]> vectors)
        {
            return Regex.Replace(input,
                @"\b(Sum|Mean|Fact|Log)\s*\[\s*([a-zA-Z_]\w*)\s*\]",
                match =>
                {
                    string op = match.Groups[1].Value;
                    string name = match.Groups[2].Value;

                    if (!vectors.TryGetValue(name, out var vector))
                        return match.Value;

                    var values = string.Join(", ",
                        vector.Select(v => v.ToString(CultureInfo.InvariantCulture)));
                    return $"{op}[{values}]";
                },
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Проверка функций вида max(a, b) или min(x, y) на совместимость векторов по размеру
        /// </summary>
        private string ValidateVectorArgumentsInFunctions(string input, Dictionary<string, double[]> vectors)
        {
            return Regex.Replace(input,
                @"\b(Max|Min|Log|If)\s*\(\s*([^)]+?)\s*\)",
                match =>
                {
                    string func = match.Groups[1].Value;
                    string args = match.Groups[2].Value;

                    var argNames = args
                        .Split(',')
                        .Select(a => a.Trim())
                        .Where(a => vectors.ContainsKey(a))
                        .ToList();

                    if (argNames.Count >= 2)
                    {
                        int length = vectors[argNames[0]].Length;
                        foreach (var arg in argNames.Skip(1))
                        {
                            if (vectors[arg].Length != length)
                            {
                                throw new ArgumentException(
                                    $"Функция {func} требует векторы одинаковой длины: '{argNames[0]}' имеет {length}, а '{arg}' — {vectors[arg].Length}.");
                            }
                        }
                    }

                    return match.Value;
                },
                RegexOptions.IgnoreCase);
        }

        private string ReplaceRegularVariables(
            string input,
            Dictionary<string, double> scalars,
            Dictionary<string, double[]> vectors)
        {
            foreach (var (key, value) in scalars)
            {
                var pattern = $@"(?<!\w){Regex.Escape(key)}(?!\w)";
                input = Regex.Replace(input, pattern,
                    value.ToString(CultureInfo.InvariantCulture),
                    RegexOptions.IgnoreCase);
            }

            foreach (var (key, vector) in vectors)
            {
                var pattern = $@"(?<!\w){Regex.Escape(key)}(?!\w)";
                var vectorText = $"[{string.Join(", ", vector.Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";
                input = Regex.Replace(input, pattern, vectorText, RegexOptions.IgnoreCase);
            }

            return input;
        }
    }
}
