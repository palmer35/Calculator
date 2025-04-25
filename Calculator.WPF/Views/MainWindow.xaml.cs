using Calculator.Parser.Calculator;
using Calculator.WPF.Models.Extractors;
using Calculator.WPF.Models.Substitutors;
using Calculator.WPF.Models.Validators;
using Calculator.WPF.Services.Calculation;
using Calculator.WPF.Services.Variables;
using ClassLibrary1;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculator.WPF.Views
{
    public partial class MainWindow
    {
        private readonly CalculationController _calculationController;
        private readonly VariableInputProcessor _inputProcessor;
        private readonly ErrorFormatter _errorFormatter;
        private readonly Dictionary<string, double[]> _vectorValues = new();

        public List<string> Variables { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация зависимостей
            _calculationController = new CalculationController(
                new ExpressionValidator(),
                new VariableExtractor(),
                new VariableSubstitutor(),
                new ExpressionParser());

            _inputProcessor = new VariableInputProcessor();
            _errorFormatter = new ErrorFormatter();
        }

        /// <summary>
        /// Обработка нажатия кнопки "Вычислить".
        /// </summary>
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            var input = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                DisplayError("Выражение не может быть пустым.", true);
                return;
            }

            ResetCalculationState();

            try
            {
                // Проверка корректности математического выражения
                _calculationController.ValidateExpression(input);

                var variables = _calculationController.GetVariables(input);

                // Если выражение содержит переменные, выводим форму для их ввода
                if (variables.Any())
                    ShowVariableInputs(variables);
                else
                    // Если переменные отсутствуют, сразу вычисляем результат
                    CalculateAndDisplayResult(input);
            }
            catch (ArgumentException ex)
            {
                DisplayError(ex.Message, true); // Ошибка валидации
            }
            catch (CalculatorException ex)
            {
                DisplayError(ex.Message); // Ошибка вычислений
            }
            catch (Exception ex)
            {
                DisplayError($"Неожиданная ошибка: {ex.Message}"); // Другие неожиданные ошибки
            }
        }

        /// <summary>
        /// Подтверждение ввода переменных и повторный расчет.
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var input = InputBox.Text.Trim();

            try
            {
                // Получение позиций переменных в выражении
                var variablesWithPositions = _calculationController.GetVariablesWithPositions(input);

                // Обработка введенных пользователем значений переменных
                var (scalarValues, errorVariables) = _inputProcessor.ProcessInputs(
                    Variables,
                    variablesWithPositions,
                    FindTextBoxForVariable);

                // Проверка на наличие ошибок при вводе переменных
                if (errorVariables != null && errorVariables.Count != 0)
                {
                    throw new CalculatorException(
                        _errorFormatter.FormatVariableError(errorVariables),
                        errorVariables);
                }

                // Обновление значений векторов и выполнение вычислений
                UpdateVectorValues();
                CalculateAndDisplayResult(input, scalarValues);
            }
            catch (CalculatorException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет расчет и выводит результат.
        /// </summary>
        private void CalculateAndDisplayResult(string expression, Dictionary<string, double>? scalarValues = null)
        {
            try
            {
                // Подстановка значений переменных в выражение
                var result = _calculationController.Calculate(
                    expression,
                    scalarValues ?? new Dictionary<string, double>(), // Скалярные значения переменных
                    _vectorValues); // Векторные значения переменных

                // Отображение результата
                switch (result)
                {
                    case double[] vectorResult:
                        DisplayResult(VectorToString(vectorResult));
                        break;

                    case double scalarResult:
                        DisplayResult(scalarResult.ToString(CultureInfo.InvariantCulture));
                        break;

                    default:
                        DisplayError("Неподдерживаемый тип результата");
                        break;
                }
            }
            catch (CalculatorException ex)
            {
                DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                DisplayError($"Ошибка вычисления: {ex.Message}");
            }
        }


        /// <summary>
        /// Сброс состояния UI перед новым вычислением
        /// </summary>
        private void ResetCalculationState()
        {
            Variables.Clear();
            VariablesItemsControl.ItemsSource = null;
            VariableInputsPanel.Visibility = Visibility.Collapsed;
            _vectorValues.Clear();
        }

        /// <summary>
        /// Отображает поля ввода переменных
        /// </summary>
        private void ShowVariableInputs(List<string> variables)
        {
            Variables = variables;
            VariablesItemsControl.ItemsSource = Variables;
            VariableInputsPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обновляет словарь векторных значений переменных
        /// </summary>
        private void UpdateVectorValues()
        {
            foreach (var kvp in _inputProcessor.GetProcessedVectors())
            {
                _vectorValues[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Преобразует вектор в строковое представление
        /// </summary>
        private string VectorToString(double[]? vector)
        {
            return vector?.Length > 0
                ? $"[{string.Join(", ", vector.Select(x => x.ToString(CultureInfo.InvariantCulture)))}]"
                : "[]";
        }

        /// <summary>
        /// Поиск текстового поля для конкретной переменной
        /// </summary>
        private TextBox? FindTextBoxForVariable(string variable)
        {
            foreach (var item in VariablesItemsControl.Items)
            {
                if (item?.ToString() == variable)
                {
                    var container = VariablesItemsControl.ItemContainerGenerator.ContainerFromItem(item);
                    if (container is ContentPresenter presenter)
                    {
                        var stackPanel = VisualTreeHelper.GetChild(presenter, 0) as StackPanel;
                        return stackPanel?.Children.OfType<TextBox>().FirstOrDefault();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Отображение результата
        /// </summary>
        private void DisplayResult(string result)
        {
            ResultText.Text = result;
            ResultText.Foreground = Brushes.Black;
        }

        /// <summary>
        /// Отображение ошибок
        /// </summary>
        private void DisplayError(string message, bool isValidationError = false)
        {
            ResultText.Text = message;
            ResultText.Foreground = isValidationError ? Brushes.Red : Brushes.Red;
        }
    }
}
