using Calculator.WPF.Models.Interfaces;
using Calculator.WPF.Models.Validators.Validators;

namespace Calculator.WPF.Models.Validators
{
    /// <summary>
    /// Комплексный валидатор для математических выражений.
    /// Объединяет набор отдельных валидаторов и последовательно применяет их.
    /// </summary>
    public class ExpressionValidator : IExpressionValidator
    {
        private readonly List<IValidator> _validators;

        public ExpressionValidator()
        {
            _validators = new List<IValidator>
            {
                new BracketsValidator(),            
                new QuotesValidator(),              
                new OperatorsValidator(),           
                new CommasValidator(),             
                new FunctionsValidator(),          
                new VariablesValidator(),           
                new NumbersValidator(),             
                new MissingOperandsValidator(),   
                new SqrtValidator(),             
                new FunctionArgumentsValidator(),   
                new FunctionSpacingValidator()     
            };
        }

        /// <summary>
        /// Выполняет последовательную валидацию входной строки с помощью всех валидаторов.
        /// </summary>
        /// <param name="input">Входное выражение</param>
        public void Validate(string input)
        {
            foreach (var validator in _validators)
            {
                validator.Validate(input);
            }
        }
    }
}