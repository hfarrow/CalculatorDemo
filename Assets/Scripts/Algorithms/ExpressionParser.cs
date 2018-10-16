using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sprache;

namespace CalculatorDemo.Algorithms
{
    /// <summary>
    /// Sprache combinator parser from
    /// https://github.com/sprache/Sprache/blob/master/samples/LinqyCalculator/ExpressionParser.cs
    /// NOTE: the link above has a bug where 'InnerTerm' calls Parse.ChainOperator instead of Parse.ChainRightOperator.
    ///     This is fixed blow. The change correctly gives pow (^) right associativity.
    ///     The linked implementation also does not support function names with numbers such as atan2 and has been fixed
    ///     as well.
    /// </summary>
    /// <remarks>
    /// The EBNF grammar for this parser is the following:
    ///    expression = term { addop term }
    ///    term = innerterm { mulop innerterm }
    ///    innerterm = { operand powop } operand
    ///    operand = ( "-" factor ) |  factor
    ///    factor = "(" expression ")" | constant | function
    ///    addop = "+" | "-"
    ///    mulop = "*" | "/" | "%"
    ///    powop = "^"
    ///    function = letter {letter_or_digit} "(" expression ")"
    /// Note: Function names are any public static methods of the Math class. Names are case insensitive
    /// </remarks>
    public static class ExpressionParser
    {
        public static Expression<Func<double>> ParseExpression(string text)
        {
            return lambda.Parse(text);
        }
        
        public static IResult<Expression<Func<double>>> TryParseExpression(string text)
        {
            return lambda.TryParse(text);
        }

        private static Parser<ExpressionType> Operator(string op, ExpressionType opType)
        {
            return Parse.String(op).Token().Return(opType);
        }

        private static readonly Parser<ExpressionType> add = Operator("+", ExpressionType.AddChecked);
        private static readonly Parser<ExpressionType> subtract = Operator("-", ExpressionType.SubtractChecked);
        private static readonly Parser<ExpressionType> multiply = Operator("*", ExpressionType.MultiplyChecked);
        private static readonly Parser<ExpressionType> divide = Operator("/", ExpressionType.Divide);
        private static readonly Parser<ExpressionType> modulo = Operator("%", ExpressionType.Modulo);
        private static readonly Parser<ExpressionType> power = Operator("^", ExpressionType.Power);

        private static readonly Parser<Expression> function =
            from first in Parse.Letter.Once().Text()
            from name in Parse.LetterOrDigit.Many().Text()
            from lparen in Parse.Char('(')
            from expr in Parse.Ref(() => expr).DelimitedBy(Parse.Char(',').Token())
            from rparen in Parse.Char(')')
            select CallFunction(first + name, expr.ToArray());

        private static Expression CallFunction(string name, Expression[] parameters)
        {
            MethodInfo methodInfo = typeof(Math).GetMethod(
                name,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static,
                null,
                parameters.Select(e => e.Type).ToArray(),
                null);
            
            if (methodInfo == null)
                throw new ParseException(string.Format("Function '{0}({1})' does not exist.", name,
                                                       string.Join(",", parameters.Select(e => e.Type.Name))));

            return Expression.Call(methodInfo, parameters);
        }

        private static readonly Parser<Expression> constant =
             Parse.Decimal
             .Select(x => Expression.Constant(double.Parse(x)))
             .Named("number");

        private static readonly Parser<Expression> factor =
            (from lparen in Parse.Char('(')
              from expr in Parse.Ref(() => expr)
              from rparen in Parse.Char(')')
              select expr).Named("expression")
             .XOr(constant)
             .XOr(function);

        private static readonly Parser<Expression> operand =
            ((from sign in Parse.Char('-')
              from factor in factor
              select Expression.Negate(factor)
             ).XOr(factor)).Token();

        private static readonly Parser<Expression> innerTerm = Parse.ChainRightOperator(power, operand, Expression.MakeBinary);

        private static readonly Parser<Expression> term = Parse.ChainOperator(multiply.Or(divide).Or(modulo), innerTerm, Expression.MakeBinary);

        private static readonly Parser<Expression> expr = Parse.ChainOperator(add.Or(subtract), term, Expression.MakeBinary);

        private static readonly Parser<Expression<Func<double>>> lambda =
            expr.End().Select(body => Expression.Lambda<Func<double>>(body));
    }
}