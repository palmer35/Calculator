using System.Linq.Expressions;

namespace Calculator.Parser.Parsers.OperatorParser
{
    public static class VectorOperations
    {
        // -------------------- Арифметика: Векторы с векторами --------------------
        
        /// <summary>
        /// Сложение двух векторов.
        /// </summary>
        public static double[] Add(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l + r).ToArray();
        }

        /// <summary>
        /// Вычитание двух векторов.
        /// </summary>
        public static double[] Subtract(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l - r).ToArray();
        }

        /// <summary>
        /// Умножение двух векторов.
        /// </summary>
        public static double[] Multiply(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l * r).ToArray();
        }

        /// <summary>
        /// Деление двух векторов.
        /// </summary>
        public static double[] Divide(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            EnsureNoZerosInVector(right);
            return left.Zip(right, (l, r) => l / r).ToArray();
        }

        // -------------------- Арифметика: Скаляр и вектор --------------------

        /// <summary>
        /// Сложение скаляра и вектора.
        /// </summary>
        public static double[] Add(double scalar, double[] vector) => vector.Select(v => scalar + v).ToArray();

        /// <summary>
        /// Сложение вектора и скаляра.
        /// </summary>
        public static double[] Add(double[] vector, double scalar) => vector.Select(v => v + scalar).ToArray();

        /// <summary>
        /// Вычитание скаляра из вектора.
        /// </summary>
        public static double[] Subtract(double scalar, double[] vector) => vector.Select(v => scalar - v).ToArray();

        /// <summary>
        /// Вычитание вектора из скаляра.
        /// </summary>
        public static double[] Subtract(double[] vector, double scalar) => vector.Select(v => v - scalar).ToArray();

        /// <summary>
        /// Умножение скаляра на вектор.
        /// </summary>
        public static double[] Multiply(double scalar, double[] vector) => vector.Select(v => scalar * v).ToArray();

        /// <summary>
        /// Умножение вектора на скаляр.
        /// </summary>
        public static double[] Multiply(double[] vector, double scalar) => vector.Select(v => v * scalar).ToArray();

        /// <summary>
        /// Деление скаляра на вектор.
        /// </summary>
        public static double[] Divide(double scalar, double[] vector)
        {
            EnsureNoZerosInVector(vector);
            return vector.Select(v => scalar / v).ToArray();
        }

        /// <summary>
        /// Деление вектора на скаляр.
        /// </summary>
        public static double[] Divide(double[] vector, double scalar)
        {
            EnsureNonZeroScalar(scalar);
            return vector.Select(v => v / scalar).ToArray();
        }

        // -------------------- Другие векторные операции --------------------

        /// <summary>
        /// Скалярное произведение двух векторов.
        /// </summary>
        public static double DotProduct(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l * r).Sum();
        }

        /// <summary>
        /// Норма (длина) вектора.
        /// </summary>
        public static double Magnitude(double[] vector) => Math.Sqrt(vector.Sum(v => v * v));

        /// <summary>
        /// Нормализация вектора.
        /// </summary>
        public static double[] Normalize(double[] vector)
        {
            var magnitude = Magnitude(vector);
            if (magnitude == 0)
                throw new InvalidOperationException("Невозможно нормализовать нулевой вектор.");
            return vector.Select(v => v / magnitude).ToArray();
        }

        /// <summary> Остаток от деления двух векторов (поэлементно) </summary>
        public static double[] Modulo(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l % r).ToArray();
        }

        /// <summary> Остаток от деления скаляра на вектор (поэлементно) </summary>
        public static double[] Modulo(double scalar, double[] vector)
        {
            return vector.Select(v => scalar % v).ToArray();
        }

        /// <summary> Остаток от деления вектора на скаляр (поэлементно) </summary>
        public static double[] Modulo(double[] vector, double scalar)
        {
            return vector.Select(v => v % scalar).ToArray();
        }


        // -------------------- Сравнения --------------------

        /// <summary>
        /// Проверка на равенство двух векторов.
        /// </summary>
        public static bool Equal(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l == r).All(result => result);
        }

        /// <summary>
        /// Проверка на неравенство двух векторов.
        /// </summary>
        public static bool NotEqual(double[] left, double[] right) => !Equal(left, right);

        /// <summary>
        /// Проверка, что первый вектор меньше второго.
        /// </summary>
        public static bool LessThan(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l < r).All(result => result);
        }

        /// <summary>
        /// Проверка, что первый вектор меньше или равен второму.
        /// </summary>
        public static bool LessThanOrEqual(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l <= r).All(result => result);
        }

        /// <summary>
        /// Проверка, что первый вектор больше второго.
        /// </summary>
        public static bool GreaterThan(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l > r).All(result => result);
        }

        /// <summary>
        /// Проверка, что первый вектор больше или равен второму.
        /// </summary>
        public static bool GreaterThanOrEqual(double[] left, double[] right)
        {
            EnsureSameLength(left, right);
            return left.Zip(right, (l, r) => l >= r).All(result => result);
        }

        /// <summary>
        /// Сравнение вектора с скаляром.
        /// </summary>
        public static bool CompareVectorToScalar(double[] vector, double scalar, ExpressionType op)
        {
            return vector.All(v => CompareScalar(v, scalar, op));
        }

        /// <summary>
        /// Сравнение скаляра с вектором.
        /// </summary>
        public static bool CompareScalarToVector(double scalar, double[] vector, ExpressionType op)
        {
            return vector.All(v => CompareScalar(scalar, v, op));
        }

        private static bool CompareScalar(double left, double right, ExpressionType op)
        {
            return op switch
            {
                ExpressionType.Equal => left == right,
                ExpressionType.NotEqual => left != right,
                ExpressionType.LessThan => left < right,
                ExpressionType.LessThanOrEqual => left <= right,
                ExpressionType.GreaterThan => left > right,
                ExpressionType.GreaterThanOrEqual => left >= right,
                _ => throw new NotSupportedException($"Оператор {op} не поддерживается."),
            };
        }

        // -------------------- Вспомогательные методы --------------------

        /// <summary>
        /// Проверка на одинаковую длину двух векторов.
        /// </summary>
        private static void EnsureSameLength(double[] left, double[] right)
        {
            if (left.Length != right.Length)
                throw new InvalidOperationException("Векторы должны быть одинаковой длины.");
        }

        /// <summary>
        /// Проверка на деление на ноль в векторе.
        /// </summary>
        private static void EnsureNoZerosInVector(double[] vector)
        {
            if (vector.Any(v => v == 0))
                throw new InvalidOperationException("Невозможно делить на ноль.");
        }

        /// <summary>
        /// Проверка, что скаляр не равен нулю.
        /// </summary>
        private static void EnsureNonZeroScalar(double scalar)
        {
            if (scalar == 0)
                throw new InvalidOperationException("Невозможно делить на ноль.");
        }

        /// <summary>
        /// Факториал одного числа.
        /// </summary>
        public static double Fact(double x)
        {
            if (x < 0 || x % 1 != 0)
                throw new InvalidOperationException("Факториал только для неотрицательных целых чисел.");
            double result = 1;
            for (int i = 2; i <= (int)x; i++)
                result *= i;
            return result;
        }

        /// <summary>
        /// Факториал каждого элемента в векторе.
        /// </summary>
        public static double[] Fact(double[] vector)
        {
            return vector.Select(Fact).ToArray();
        }

    }
}
