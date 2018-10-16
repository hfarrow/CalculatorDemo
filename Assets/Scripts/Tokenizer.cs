using System.Collections.Generic;
using System.Text;

namespace CalculatorDemo
{
    /// <summary>
    /// A basic numerix expression tokenizer. See Token.cs for parsable tokens.
    /// The tokenizer walks through an expression (source string) one character at a time outputs a list of in order
    /// tokens. The tokenizer does not valid the syntax or correctness of the produced tokens.
    /// </summary>
    public static class Tokenizer
    {
        public static Result<List<Token>> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            int currentIndex = 0;
            while (currentIndex < expression.Length)
            {               
                char currentChar = expression[currentIndex];

                // Skip whitespace
                if (currentChar == ' ')
                {
                    ++currentIndex;
                }
                // The start of a number (double)
                else if (char.IsDigit(currentChar) || currentChar == '.')
                {
                    Token numberToken = TokenizeNumber(expression, currentIndex);
                    tokens.Add(numberToken);
                    currentIndex = numberToken.EndIndex + 1;
                }
                // The start of a function or variable. (Only functions are implemented in this tokenizer)
                else if (char.IsLetter(currentChar))
                {
                    int startIndex = currentIndex;
                    while (currentIndex < expression.Length &&
                           (char.IsLetter(expression[currentIndex]) || char.IsDigit(expression[currentIndex])))
                    {
                        ++currentIndex;
                    }


                    // Presence of a lparen indicates a function
                    if (currentIndex < expression.Length && expression[currentIndex] == '(')
                    {
                        var functionToken = new Token(
                            TokenType.Function,
                            startIndex, currentIndex - startIndex,
                            expression);
                        tokens.Add(functionToken);
                    }
                    else
                    {
                        // Lack of a lparen indicates a variable
                        tokens.Add(new Token(TokenType.Variable, startIndex, currentIndex - startIndex, expression));
                    }
                    
                }
                else if (currentChar == '(')
                {
                    tokens.Add(new Token(TokenType.LParen, currentIndex, 1, expression));
                    ++currentIndex;
                }
                else if (currentChar == ')')
                {
                    tokens.Add(new Token(TokenType.RParen, currentIndex, 1, expression));
                    ++currentIndex;
                }
                else if (currentChar == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, currentIndex, 1, expression));
                    ++currentIndex;
                }
                else if (Operator.Operators.ContainsKey(currentChar))
                {
                    // Note that operators are limited to a single character for simplicity.
                    Token? previous = null;
                    if (tokens.Count > 0)
                    {
                        previous = tokens[tokens.Count - 1];
                    }

                    // Unary minus (negate)
                    // It is only a unary operator if it does not follow a number or right parenthesis.
                    if (currentChar == '-' && (!previous.HasValue ||
                                               (previous.Value.Type != TokenType.Number &&
                                                previous.Value.Type != TokenType.RParen)))
                    {
                        // Replace unary minus with tilde. If operators were not constrained to a single character then
                        // using a string like "-u" would be more intuitive.
                        expression = new StringBuilder(expression) {[currentIndex] = '~'}.ToString();
                        tokens.Add(new Token(TokenType.Operator, currentIndex, 1, expression));
                    }
                    // Binary minus (substract)
                    else
                    {
                        tokens.Add(new Token(TokenType.Operator, currentIndex, 1, expression));
                    }
                    
                    ++currentIndex;
                }
                else
                {
                    // Invalid character means the entire expression is invalid
                    return Result.FromError<List<Token>>(
                        string.Format("Invalid character '{0}' at index {1}. Previous token was {2}",
                            currentChar, currentIndex, tokens[tokens.Count - 1]));
                }
            }

            return Result.FromValue(tokens);
        }

        private static Token TokenizeNumber(string expression, int currentIndex)
        {
            // Walk forward through the expression until a non numeric character is found.
            int startIndex = currentIndex;
            while (currentIndex < expression.Length && 
                   (char.IsDigit(expression[currentIndex]) || expression[currentIndex] == '.'))
            {
                ++currentIndex;
            }

            return new Token(TokenType.Number, startIndex, currentIndex - startIndex, expression);
        }
    }
}