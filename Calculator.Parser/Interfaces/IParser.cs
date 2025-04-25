using System.Linq.Expressions;

namespace Calculator.Parser.Interfaces
{
    /// <summary>
    /// Интерфейс для парсера, который преобразует строку в выражение.
    /// </summary>
    public interface IParser<TResult>
    {
        /// <summary>
        /// Преобразует входную строку в выражение.
        /// </summary>
        Expression<Func<TResult>> Parse(string input);
    }
}