using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CalculatorDemo
{
    public enum Associativity
    {
        Right,
        Left
    }

    /// <summary>
    /// A struct representing Unary and Binary operators: + - / * % ^
    /// The Operate and ToExpression delegates take an RPN stack and pop any arguments the operator will consume.
    /// </summary>
    public struct Operator
    {
        public static readonly IReadOnlyDictionary<char, Operator> Operators =
            new[]
            {
                new Operator('+', 2, Associativity.Left,
                    operands => operands.Pop() + operands.Pop(),
                    operands => Expression.Add(operands.Pop(), operands.Pop())),
                
                new Operator('-', 2, Associativity.Left, operands =>
                    {
                        double rhs = operands.Pop();
                        return operands.Pop() - rhs;
                    },
                    operands =>
                    {
                        Expression rhs = operands.Pop();
                        return Expression.Subtract(operands.Pop(), rhs);
                    }),
                
                new Operator('*', 3, Associativity.Left,
                    operands => operands.Pop() * operands.Pop(),
                    operands => Expression.Multiply(operands.Pop(), operands.Pop())),
                
                new Operator('/', 3, Associativity.Left, operands =>
                    {
                        double rhs = operands.Pop();
                        return operands.Pop() / rhs;
                    },
                    operands =>
                    {
                        Expression rhs = operands.Pop();
                        return Expression.Divide(operands.Pop(), rhs);
                    }),
                
                new Operator('%', 3, Associativity.Left, operands =>
                    {
                        double rhs = operands.Pop();
                        return operands.Pop() % rhs;
                    },
                    operands =>
                    {
                        Expression rhs = operands.Pop();
                        return Expression.Modulo(operands.Pop(), rhs);
                    }),
                
                new Operator('^', 4, Associativity.Right, operands =>
                    {
                        double rhs = operands.Pop();
                        return Math.Pow(operands.Pop(), rhs);
                    },
                    operands =>
                    {
                        Expression rhs = operands.Pop();
                        return Expression.Power(operands.Pop(), rhs);
                    }),
                
                // The unary negate operator is represented by '~' in order to distinguish it from subtract.
                // The '~' is an internal representation. The original infix expression uses '-' as would be expected.
                new Operator('~', 5, Associativity.Right,
                    operands => -(operands.Pop()),
                    operands => Expression.Negate(operands.Pop()))
                
            }.ToDictionary(op => op.Symbol, op => op);

        public static Operator FromToken(Token token)
        {
            if (token.Type != TokenType.Operator)
            {
                throw new ArgumentException($"The provided token '{token}' must be an operator", nameof(token));
            }

            return Operators[token.SourceSlice[0]];
        }
        
        public readonly char Symbol;
        public readonly int Precedence;
        public readonly Associativity Associativity;
        public readonly Func<Stack<double>, double> Operate;
        public readonly Func<Stack<Expression>, Expression> ToExpression;

        public Operator(char symbol, int precedence, Associativity associativity,
            Func<Stack<double>, double> operate,
            Func<Stack<Expression>, Expression> toExpression)
        {
            Symbol = symbol;
            Precedence = precedence;
            Associativity = associativity;
            Operate = operate;
            ToExpression = toExpression;
        }
    }
}