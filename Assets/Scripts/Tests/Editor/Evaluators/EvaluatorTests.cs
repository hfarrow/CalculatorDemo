using System.Collections.Generic;
using CalculatorDemo.Evaluators;
using NUnit.Framework;
using Sprache;
using UnityEngine;

namespace CalculatorDemo.Tests.Evaluators
{
    public class EvaluatorTests
    {
        private readonly IReadOnlyCollection<IEvaluator> evaluators = new IEvaluator[]
        {
            new CombinatorParserEvaluator(),
            new ReversePolishNotationEvaluator()
        };

        [Test, Sequential]
        public void CanEvaluateValidInput(
            [Values(
                "3 + 4 * 2 / ( 1 - 5 ) ^ 2 ^ 3",
                "3 ^ 2 ^ ( 5 - 1) / 2 * 4 + 3",
                "7 / ( 3 ^ 2 ) - 2 * 2",
                "7 / ( 3 ^ -2 ) - 2 * 2",
                "-4 + 2")] // Can start with negate unary operator
            string expression,
            [Values(
                3.00012207f,
                86093445f,
                -3.222222f,
                59f,
                -2f)]
            float expected)
        {
            foreach (IEvaluator evaluator in evaluators)
            {
                double result = evaluator.Evaluate(expression);
                Debug.LogFormat(
                    "{0} | expected: {1} actual: {2}",
                    evaluator.GetType().Name, expected, (float)result);
                Assert.True(Mathf.Approximately(expected, (float) result));
            }
        }

        [Test, Sequential]
        public void InvalidExpressionThrows(
            [Values(
                "3 + 4 * 2 / ( 1 - 5 ^ 2 ^ 3",
                "3 + 4 * 2 / 1 - 5 ) ^ 2 ^ 3",
                "3 $ 4")]
            string expression)
        {
            foreach (IEvaluator evaluator in evaluators)
            {
                Debug.Log(evaluator.GetType().Name);
                if (evaluator is CombinatorParserEvaluator)
                {
                    Assert.Throws<ParseException>(() => evaluator.Evaluate(expression));
                }
                else if (evaluator is ReversePolishNotationEvaluator)
                {
                    Assert.Throws<EvaluationException>(() => evaluator.Evaluate(expression));
                }
            }
        }

        [Test, Sequential]
        public void CanCallFunctions(
            [Values(
                "pow(2, 2)",
                "abs(-1)",
                "floor(1.6)"
                )]
            string expression,
            [Values(
                4f,
                1f,
                1f
                )]
            float expectedResult)
        {
            foreach (IEvaluator evaluator in evaluators)
            {
                Debug.Log(evaluator.GetType().Name);
                double result = evaluator.Evaluate(expression);
                Assert.True(Mathf.Approximately(expectedResult, (float) result));
            }
        }

        [Test, Sequential]
        public void CanCallCompositeFunctions(
            [Values(
                "pow(pow(2, 2), 2)",
                "pow(pow(2, 2), pow(2, 2))",
                "pow(pow(1, pow(3, 2)), 1)"
                )]
            string expression,
            [Values(
                16f,
                256f,
                1f
                )]
            float expectedValue)
        {
            foreach (IEvaluator evaluator in evaluators)
            {
                Debug.Log(evaluator.GetType().Name);
                double result = evaluator.Evaluate(expression);
                Assert.True(Mathf.Approximately(expectedValue, (float) result));
            }
        }

        [Test]
        public void CanCallChainedFunctions()
        {
            const string expression = "pow(2 + 2, 2) - 8";
            foreach (IEvaluator evaluator in evaluators)
            {
                Debug.Log(evaluator.GetType().Name);
                double result = evaluator.Evaluate(expression);
                Assert.True(Mathf.Approximately(8f, (float) result));
            }
        }

        [Test]
        public void CanWrapFunctionArgumentsInParentheses()
        {
            const string expression = "pow((2 + 2), (2))";
            foreach (IEvaluator evaluator in evaluators)
            {
                Debug.Log(evaluator.GetType().Name);
                double result = evaluator.Evaluate(expression);
                Assert.True(Mathf.Approximately(16f, (float) result));
            }
        }
    }
}