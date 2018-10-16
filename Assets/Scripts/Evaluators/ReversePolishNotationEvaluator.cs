using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CalculatorDemo.Algorithms;
using UnityEngine;

namespace CalculatorDemo.Evaluators
{
	/// <summary>
	/// A numeric expression evaluator that takes input in the form of an infix notation string.
	/// The input expression is then converted to reverse polish notation (postfix notation). Finally the expression
	/// is converted to an expression tree that is ready for evaluation.
	/// </summary>
	/// <remarks>
	/// Converting the input expression to an expression tree is not required. The stack of expressions can be changed
	/// to a stack of doubles. The result of each operator is then pushed onto the stack. Operator.Operate(stack) performs
	/// such an operation. Operator.ToExpression(stack) is used to get the expression an operator represents.
	/// </remarks>
	public class ReversePolishNotationEvaluator : IEvaluator
	{
		/// <inheritdoc />
		public double Evaluate(string expression)
		{
			Result<Expression<Func<double>>> result = TryEvaluateToExpressionTree(expression);
			if (!result.HasValue)
			{
				throw new EvaluationException(expression, result.ErrorMessage);
			}

			return result.Value.Compile()();
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
			Result<List<Token>> rpnResult = ShuntingYard.InfixToPostfixTokens(expression);
			if (!rpnResult.HasValue)
			{
				return Result.FromError<Expression<Func<double>>>(rpnResult.ErrorMessage);
			}
			
			var stack = new Stack<Expression>();
			foreach (Token token in rpnResult.Value)
			{
				// Convert each token to an expression node and push it onto the stack.
				// As noted in comments below. Some nodes will consume other nodes by popping them off of the stack.
				switch (token.Type)
				{
					case TokenType.Number:
					// Numbers are transformed into an expression constant and pushed onto the stack.
						double value = double.Parse(token.SourceSlice);
						stack.Push(Expression.Constant(value));
						break;
					case TokenType.Function:
					// Function consume their arguments by popping them from the stack and pushing the resulting
					// function call expression onto the stack.
						List<Expression> args;
						Result<Expression> functionResult = GenerateFunctionCallExpression(token, stack, out args);
						if (!functionResult.HasValue)
						{
							string message = string.Format("Failed to generate function call to '{0}({1}): {2}",
								token.SourceSlice, string.Join(", ", args), functionResult.ErrorMessage);
							return Result.FromError<Expression<Func<double>>>(message);
						}
                        stack.Push(functionResult.Value);
						break;
					case TokenType.Operator:
					// Operators are similar to funtions in that they consume their operands by popping them from the
					// stack and pushing the resulting operator node onto the stack.
						Operator op = Operator.FromToken(token);
						try
						{
                            stack.Push(op.ToExpression(stack));
						}
						catch (InvalidOperationException)
						{
							return Result.FromError<Expression<Func<double>>>(
								string.Format("Invalid expression '{0}'. " +
								              "Missing one or more operands near token {1}",
									expression, token));
						}
						break;
					default:
						return Result.FromError<Expression<Func<double>>>(
							$"Unexpected token '{token}' found in RPN expression.");
				}
			}

			if (stack.Count != 1)
			{
				string message = string.Format("Invalid expression '{0}'", expression);
				return Result.FromError<Expression<Func<double>>>(message);
			}

			return Result.FromValue(Expression.Lambda<Func<double>>(stack.Pop()));
		}

		/// <summary>
		/// Generate a function call expression after determining the number of arguments from the stack to
		/// consume. The consumed arguments are popped from the stack and passed into the function call expression node.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="stack"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private Result<Expression> GenerateFunctionCallExpression(Token token, Stack<Expression> stack, out List<Expression> args)
		{
			args = new List<Expression>();
			
			// When the function was tokenized the number of arguments for the function was added as a 
			// metadata property by the tokenizer. The Reverse Polish notation used here does not indicate
			// how many expressions from the stack should be consumed by the function call. Metadata is
			// required to track the number of arguments. Alternatively, the notation could include an
			// "end function" token.
			int numArgs;
			token.TryGetMetadataProperty("argCount", out numArgs);
			if (numArgs > stack.Count)
			{
				string message = string.Format("Not enough arguments for function '{0}' at index {1}",
					token.SourceSlice, token.Index);
				return Result.FromError<Expression>(message);
			}

			for (int i = 0; i < numArgs; ++i)
			{
				args.Add(stack.Pop());
			}

			args.Reverse();
			return GetCallMethodExpression(token, args.ToArray());
		}

		private Result<Expression> GetCallMethodExpression(Token functionToken, Expression[] parameters)
		{
			string name = functionToken.SourceSlice;
			MethodInfo methodInfo = Utils.GetMathFunction(name, parameters.Length);
			if (methodInfo == null)
			{
				return Result.FromError<Expression>(string.Format("Function '{0}({1})' does not exist.",
					name, string.Join(",", parameters.Select(e => e.Type.Name))));
			}

			return Result.FromValue<Expression>(Expression.Call(methodInfo, parameters));
		}
	}
}
