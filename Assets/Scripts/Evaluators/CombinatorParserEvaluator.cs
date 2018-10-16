using System;
using System.Linq.Expressions;
using CalculatorDemo.Algorithms;
using Sprache;

namespace CalculatorDemo.Evaluators
{
    /// <summary>
	/// A numeric expression evaluator that takes input in the form of an infix notation string expression.
	/// The expression is then parsed by a combinator parser written using Sprache into an expression tree.
	/// NOTE: this uses a third party library and was added in addition to the RPN Evaluator that does not use
	/// any third party libraries.
    /// </summary>
    /// <remarks>
    /// Using a combinator parser may not always be the most performant solution but they do not require much
    /// code and it is very easy to see the relationship between an EBNF spec and the individual parts of the parser.
    /// The actual parser in ExpressionParser.cs
    /// </remarks>
    public class CombinatorParserEvaluator : IEvaluator
    {
		/// <inheritdoc />
        public double Evaluate(string expression)
        {
            Expression<Func<double>> exp = ExpressionParser.ParseExpression(expression);
            return exp.Compile()();
        }

		/// <inheritdoc />
        public Result<double> TryEvaluate(string expression)
        {
            Result<Expression<Func<double>>> result = TryEvaluateToExpressionTree(expression);
            if (!result.HasValue)
            {
                return Result.FromError<double>(result.ErrorMessage);
            }

            return Result.FromValue(result.Value.Compile()());
        }

		/// <inheritdoc />
        public Result<Expression<Func<double>>> TryEvaluateToExpressionTree(string expression)
        {
            try
            {
                IResult<Expression<Func<double>>> result = ExpressionParser.TryParseExpression(expression);
                if (!result.WasSuccessful)
                {
                    return Result.FromError<Expression<Func<double>>>(result.Message);
                }

                return Result.FromValue(result.Value);
            }
            catch (Exception e)
            {
                return Result.FromError<Expression<Func<double>>>(e.Message);
            }
        }
    }
}