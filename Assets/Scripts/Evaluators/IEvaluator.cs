using System;
using System.Linq.Expressions;

namespace CalculatorDemo.Evaluators
{
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluate a numeric expression written in standard infix notation. May throw an exception if the expression
        /// is invalid.
        /// </summary>
        /// <param name="expression">Standard infix notation expression such as "floor(1 + 2 / (3 + 4))"</param>
        /// <returns>The evaluated value of the expression.</returns>
        double Evaluate(string expression);

        /// <summary>
        /// Evaluate a numeric expression written in standard infix notation. If the expression is invalid then an
        /// error message is returned.
        /// </summary>
        /// <param name="expression">Standard infix notation expression such as "floor(1 + 2 / (3 + 4))"</param>
        /// <returns>On success a result containing the evaluated value and on error an error message.</returns>
        Result<double> TryEvaluate(string expression);

        /// <summary>
        /// Parse a numeric expression written in standard infix notation into an expression tree. If the expression
        /// is invalid then an error message is returned.
        /// </summary>
        /// <param name="expression">Standard infix notation expression such as "floor(1 + 2 / (3 + 4))"</param>
        /// <returns>On success a result containing the expression tree and on error an error message.</returns>
        Result<Expression<Func<double>>> TryEvaluateToExpressionTree(string expression);
    }
}