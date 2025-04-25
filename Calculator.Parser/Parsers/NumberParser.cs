using Sprache;
using System.Globalization;
using System.Linq.Expressions;

namespace Calculator.Parser.Parsers
{
    internal static class NumberParser
    {
        // Парсер для числа (скалярное или векторное)
        public static readonly Parser<Expression> Number;
        
        // Парсер для скалярных чисел
        public static readonly Parser<Expression> ScalarNumber;
        
        // Парсер для векторных чисел
        public static readonly Parser<Expression> VectorNumber;

        static NumberParser()
        {
            // Скалярное число: Парсим числа с запятой или точкой
            ScalarNumber = Parse.Regex(@"\d+([.,]\d+)?")
                .Text()
                .Select(num => num.Replace(',', '.')) // Заменяем запятую на точку
                .Select(CreateScalarExpression);

            // Вектор чисел: Парсим список чисел, заключенных в квадратные скобки
            VectorNumber =
                from openBracket in Parse.Char('[')
                from values in ScalarNumber.DelimitedBy(Parse.Char(',').Token()) // Числа разделены запятой
                from closeBracket in Parse.Char(']')
                select CreateVectorExpression(values.ToArray());

            // Число: Это либо скаляр, либо вектор
            Number = ScalarNumber.Or(VectorNumber);
        }

        // Создание выражения для скалярного числа
        private static Expression CreateScalarExpression(string numberText)
        {
            numberText = numberText.Replace(',', '.');  // Преобразуем запятую в точку для корректного парсинга
            if (double.TryParse(numberText, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            {
                return Expression.Constant(number);  // Возвращаем число как константу
            }
            throw new FormatException($"Некорректный формат числа: {numberText}");
        }

        // Создание выражения для вектора чисел
        private static Expression CreateVectorExpression(Expression[] values)
        {
            if (values.Length == 0)
            {
                throw new FormatException("Вектор не может быть пустым.");
            }
            return Expression.NewArrayInit(typeof(double), values);  // Создаем новый массив типа double
        }
    }

    public static class ParseExtensions
    {
        // Расширение для парсера, которое заменяет запятую на точку
        public static Parser<string> WithComma(this Parser<string> parser)
        {
            return parser.Select(num => num.Replace(',', '.'));  // Заменяем запятую на точку
        }
    }
}
