using System.Collections.Generic;
using NUnit.Framework;

namespace CalculatorDemo.Tests
{
    public class TokenizerTests
    {
        [Test, Sequential]
        public void CanParseValidExpression(
            [Values(
                /*0*/ "1+2",
                /*1*/ "(1+2)",
                /*2*/ " 1 + 2 ",
                /*3*/ "1-0",
                /*4*/ "1*2",
                /*5*/ "1/2",
                /*6*/ "1^2",
                /*7*/ "1.0+2.0",
                /*8*/ "1+2-3*4/5^6",
                /*9*/ "1+2-(3*4/(5^6))",
                /*10*/ "1.0+2-(3.0*4/(5.0^6))")]
            string expression,
            [Values(
                /*0*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*1*/ new[]{TokenType.LParen, TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.RParen},
                /*2*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*3*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*4*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*5*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*6*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*7*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*8*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number},
                /*9*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Operator, TokenType.LParen,
                    TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Operator, TokenType.LParen,
                    TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.RParen, TokenType.RParen},
                /*10*/ new[]{TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Operator, TokenType.LParen,
                    TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Operator, TokenType.LParen,
                    TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.RParen, TokenType.RParen})]
            TokenType[] expectedTokens)
        {
            Result<List<Token>> result = Tokenizer.Tokenize(expression);
            Assert.True(result.HasValue);

            for (int i = 0; i < expectedTokens.Length; ++i)
            {
                TokenType expected = expectedTokens[i];
                TokenType actual = result.Value[i].Type;
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void SourceSlicesAreCorrect()
        {
            const string expression = "10.01+(1-2)";
            Result<List<Token>> result = Tokenizer.Tokenize(expression);
            Assert.True(result.HasValue);

            var expectedTokens = new[]
            {
                new Token(TokenType.Number, 0, 5, expression),
                new Token(TokenType.Operator, 5, 1, expression), 
                new Token(TokenType.LParen, 6, 1, expression), 
                new Token(TokenType.Number, 7, 1, expression), 
                new Token(TokenType.Operator, 8, 1, expression), 
                new Token(TokenType.Number, 9, 1, expression), 
                new Token(TokenType.RParen, 10, 1, expression), 
            };

            Assert.AreEqual(expectedTokens.Length, result.Value.Count);
            for(int i = 0; i < expectedTokens.Length; ++i)
            {
                Assert.AreEqual(expectedTokens[i].Index, result.Value[i].Index);
                Assert.AreEqual(expectedTokens[i].Length, result.Value[i].Length);
                Assert.AreEqual(expectedTokens[i].SourceSlice, result.Value[i].SourceSlice);
            }
        }

        [Test]
        public void InvalidTokenProducesError()
        {
            const string expression = "10 $ 10";
            Result<List<Token>> result = Tokenizer.Tokenize(expression);
            Assert.False(result.HasValue);
            Assert.AreEqual("Invalid character '$' at index 3. Previous token was '10' at index 0", result.ErrorMessage);
        }

        [Test, Sequential]
        public void CanParseFunctionCalls(
            [Values(
                "pow(2, 2)",
                "pow(pow(2, 2), 2)",
                "pow(4, 2) - 8"
                )]
            string expression,
            [Values(
                new []{TokenType.Function, TokenType.LParen, TokenType.Number, TokenType.Comma, TokenType.Number,
                    TokenType.RParen},
                new []{TokenType.Function, TokenType.LParen, TokenType.Function, TokenType.LParen, TokenType.Number,
                    TokenType.Comma, TokenType.Number, TokenType.RParen, TokenType.Comma, TokenType.Number,
                    TokenType.RParen},
                new []{TokenType.Function, TokenType.LParen, TokenType.Number, TokenType.Comma, TokenType.Number,
                    TokenType.RParen, TokenType.Operator, TokenType.Number}
                )]
            TokenType[] expectedTokens)
        {
            Result<List<Token>> result = Tokenizer.Tokenize(expression);
            Assert.True(result.HasValue);

            for (int i = 0; i < expectedTokens.Length; ++i)
            {
                TokenType expected = expectedTokens[i];
                TokenType actual = result.Value[i].Type;
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void CanParseVariableName()
        {
            const string expression = "1 + var + 1";
            Result<List<Token>> result = Tokenizer.Tokenize(expression);
            Assert.True(result.HasValue);

            var expectedTokens = new[]
            {
                TokenType.Number, TokenType.Operator, TokenType.Variable, TokenType.Operator, TokenType.Number
            };
            for (int i = 0; i < expectedTokens.Length; ++i)
            {
                TokenType expected = expectedTokens[i];
                TokenType actual = result.Value[i].Type;
                Assert.AreEqual(expected, actual);
            }
            
            Assert.AreEqual("var", result.Value[2].SourceSlice);
        }
    }
}