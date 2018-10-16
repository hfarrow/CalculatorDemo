﻿using System.Collections.Generic;
using System.Linq;

namespace CalculatorDemo.Algorithms
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    /// The shunting-yard algorithm (Edsger Dijkstra) parses a mathematical expression specified in infix notation.
    /// The algorithm is capable of producing postfix notation (Reverse Polish notation) or an abstract syntax tree.
    /// This is an extended variation of the alogorithm described in the link above. It supports unary negate operator
    /// and composite functions. Support for variables such as PI or user defined variable can be easily added in the
    /// future.
    /// </summary>
    public static class ShuntingYard
    {
        /// <summary>
        /// Given an infix expression, a list of tokens in Reverse Polish notation (postfix notation) will be produced.
        /// The produced tokens are ready for an evaluator to process.
        /// </summary>
        /// <param name="expression">infix expression</param>
        /// <returns>postfix expression tokens</returns>
        public static Result<List<Token>> InfixToPostfixTokens(string expression)
        {
            Result<List<Token>> tokenizeResult = Tokenizer.Tokenize(expression);
            if (!tokenizeResult.HasValue)
            {
                // Propegate the error message upwards
                return Result.FromError<List<Token>>(tokenizeResult.ErrorMessage);
            }

            var operatorStack = new Stack<Token>();
            var output = new List<Token>();

            // Walk through all tokens... Operators including lparen and function will be pushed onto an operator stack.
            // As operators, rparen, and commas are encountered, elements from the stack will be popped into the output
            // list based on several rules that apply opertor precedence and associativity.
            for (int i = 0; i < tokenizeResult.Value.Count; i++)
            {
                Token token = tokenizeResult.Value[i];
                switch (token.Type)
                {
                    case TokenType.Number:
                        output.Add(token);
                        break;

                    case TokenType.Variable:
                        // To support variables write a function to return the variable's double value.
                        // output.Add(EvaluateVariable(token))
                        // break;
                        return Result.FromError<List<Token>>(
                            string.Format("Variable token '{0}' is not supported", token));

                    case TokenType.Function:
                        operatorStack.Push(token);
                        token.SetMetadataProperty("argCount", CountFunctionArgs(tokenizeResult.Value, i));
                        break;

                    case TokenType.Operator:
                        ProcessOperatorToken(token, operatorStack, output);
                        break;
                    case TokenType.LParen:
                        operatorStack.Push(token);
                        break;
                    case TokenType.RParen:
                    case TokenType.Comma:
                        Result<List<Token>> result = ProcessRParenOrCommaToken(token, operatorStack, output);
                        // An error result can be generated by an rparen or comma.
                        if (result != null)
                        {
                            return result;
                        }

                        break;
                    default:
                        return Result.FromError<List<Token>>("Invalid token type: " + token);
                }
            }

            // Move remaining tokens to the output.
            while (operatorStack.Count > 0)
            {
                Token topToken = operatorStack.Peek();
                if (topToken.Type == TokenType.LParen)
                {
                    return Result.FromError<List<Token>>(
                        string.Format("No matching right parenthesis for left parenthesis at index {0}",
                            topToken.Index));
                }
                
                output.Add(operatorStack.Pop());
            }

            return Result.FromValue(output);
        }

        private static void ProcessOperatorToken(Token opToken, Stack<Token> operatorStack, List<Token> output)
        {
            if (operatorStack.Count > 0)
            {
                Token topToken = operatorStack.Peek();
                Operator op = Operator.FromToken(opToken);
                Operator topOp = topToken.Type == TokenType.Operator
                    ? Operator.FromToken(topToken)
                    : default(Operator);

                // When an operator is encountered, pop tokens from the stack and add to output while
                // the conditions below are true. This is where operator precedence and associativity are taken
                // into consideration.
                while ((topToken.Type == TokenType.Function ||
                        (topToken.Type == TokenType.Operator && topOp.Precedence > op.Precedence) ||
                        (topToken.Type == TokenType.Operator && topOp.Precedence == op.Precedence &&
                         topOp.Associativity == Associativity.Left)) &&
                       topToken.Type != TokenType.LParen)
                {
                    output.Add(operatorStack.Pop());

                    if (operatorStack.Count == 0)
                    {
                        break;
                    }
                    
                    // Prepare for the next loop iteration.
                    topToken = operatorStack.Peek();
                    topOp = topToken.Type == TokenType.Operator
                        ? Operator.FromToken(topToken)
                        : default(Operator);
                }
            }

            operatorStack.Push(opToken);
        }

        private static Result<List<Token>> ProcessRParenOrCommaToken(Token token, Stack<Token> operatorStack, List<Token> output)
        {
            // Add stack to output until lparen is found.
            while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LParen)
            {
                output.Add(operatorStack.Pop());
            }

            if (operatorStack.Count == 0)
            {
                return Result.FromError<List<Token>>(
                    string.Format("No matching left parenthesis for right parenthesis at index {0}.",
                        token.Index));
            }

            // If the provided token was an rparen, pop the lparen which is now at the top of the stack.
            // Note, if the provided token was a comma do not pop the lparen yet. The remaining function arguments
            // need to be parsed first. Eventually the matching rparen will be encountered and the code below will
            // execute.
            if (token.Type == TokenType.RParen)
            {
                operatorStack.Pop();
                
                // if the lparen was part of a function call, pop the function and add it to the output.
                if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.Function)
                {
                    output.Add(operatorStack.Pop());
                }
            }

            // no result on success.
            return null;
        }

        /// <summary>
        /// Given an infix expression, a Reverse Polish notation (postfix notation) expression will be produced.
        /// </summary>
        /// <param name="expression">infix expression</param>
        /// <returns>postfix expression</returns>
        public static Result<string> InfixToPostfixStr(string expression)
        {
            Result<List<Token>> result = InfixToPostfixTokens(expression);
            if (!result.HasValue)
            {
                return Result.FromError<string>(result.ErrorMessage);
            }

            return Result.FromValue(PostfixTokensToStr(result.Value));
        }

        /// <summary>
        /// Convert a token list to a postfix string expression
        /// </summary>
        /// <param name="tokens">token to convert to a string</param>
        /// <returns></returns>
        public static string PostfixTokensToStr(List<Token> tokens)
        {
            return string.Join(" ", tokens.Select(t => t.SourceSlice));
        }

        /// <summary>
        /// Given a token list and a starting index for a function, count how many arguments the function requires.
        /// Essentially, find the matching rparen and count the commas at the top level.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="functionIndex"></param>
        /// <returns></returns>
        private static int CountFunctionArgs(List<Token> tokens, int functionIndex)
        {
            int count = 0;
            int depth = 0;
            for (int i = functionIndex; i < tokens.Count; ++i)
            {
                Token token = tokens[i];
                switch (token.Type)
                {
                    case TokenType.LParen:
                        ++depth;
                        break;
                    case TokenType.RParen:
                        --depth;
                        if (i == functionIndex + 1)
                        {
                            return 0;
                        }
                        if (depth == 0)
                        {
                            return count + 1;
                        }
                        break;
                    case TokenType.Comma:
                        if (depth == 1)
                        {
                            ++count;
                        }

                        break;
                }
            }
    
            // Did not find matching rparen
            return count + 1;
        }
    }
}