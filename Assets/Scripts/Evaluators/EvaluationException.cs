using System;

namespace CalculatorDemo.Evaluators
{
    public sealed class EvaluationException : Exception
    {
        public EvaluationException(string expression, string errorMessage)
            : base($"There was an error evaluating expression '{expression}': {errorMessage}")
        {
        }
        
    }
}