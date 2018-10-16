using CalculatorDemo.Algorithms;
using NUnit.Framework;
using UnityEngine;

namespace CalculatorDemo.Tests.Algorithms
{
    public class ShuntingYardTests
    {
        [Test]
        public void CanProducePostfixNotation()
        {
            const string expression = "3 + 4 * 2 / ( 1 - 5 ) ^ 2 ^ 3";
            Result<string> result = ShuntingYard.InfixToPostfixStr(expression);
            if (!result.HasValue)
            {
                Debug.LogError("ShuntingYard.InfixToPostfix(" + expression + ") Error: " + result.ErrorMessage);
            }
            Assert.True(result.HasValue);
            Assert.AreEqual("3 4 2 * 1 5 - 2 3 ^ ^ / +", result.Value);
        }

        [Test]
        public void CanProducePostfixNotationWithFunction()
        {
            const string expression = "sin(max(2, 3) / 3 * 3.1415)";
            Result<string> result = ShuntingYard.InfixToPostfixStr(expression);
            if (!result.HasValue)
            {
                Debug.LogError("ShuntingYard.InfixToPostfix(" + expression + ") Error: " + result.ErrorMessage);
            }
            Assert.True(result.HasValue);
            Assert.AreEqual("2 3 max 3 / 3.1415 * sin", result.Value);
        }

        [Test]
        public void NoMatchingLParenProducesError()
        {
            const string expression = "1+2)+3";
            Result<string> result = ShuntingYard.InfixToPostfixStr(expression);
            Assert.False(result.HasValue);
            Assert.True(result.ErrorMessage.StartsWith("No matching left parenthesis"));
        }
        
        [Test]
        public void NoMatchingRParenProducesError()
        {
            const string expression = "1+(2+3";
            Result<string> result = ShuntingYard.InfixToPostfixStr(expression);
            Assert.IsFalse(result.HasValue);
            Assert.True(result.ErrorMessage.StartsWith("No matching right parenthesis"));
        }
    }
}