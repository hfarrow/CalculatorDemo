using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CalculatorDemo
{
    /// <summary>
    /// A simple expression generator that produces an expression tree which can be converted to
    /// an infix notation expression string.
    /// </summary>
    public static class ExpressionGenerator
    {
        private static readonly Random rand = new Random();
        private static readonly string[] functions =
            {"pow", "abs", "max", "min", "floor", "ceiling", "cos", "sin", "tan", "atan", "atan2"};

        /// <summary>
        /// Generate a random infix notation expression given several control inputs.
        /// </summary>
        /// <param name="leafPropability">How likely that a terminal node will be generated at any depth.</param>
        /// <param name="functionPropability">How likely that a function node will be generated instead of a binary operator node.</param>
        /// <param name="maxDepth">The maximum depth of the generated tree. At max depth, a terminal node (number) will always be generatd.</param>
        /// <returns>Returns the root expression tree node. Use Node.ToString() to produce the expression.</returns>
        public static Node Generate(double leafPropability, double functionPropability, int maxDepth)
        {
            return Generate(leafPropability, functionPropability, maxDepth, true);
        }
        
        private static Node Generate(double leafPropability, double functionPropability,  int maxDepth, bool isRoot)
        {
            // Never generate a leaf node if this is the root node.
            // Always generate a leaf node if this node is at the max depth.
            // All other times, randomly generate a leaf node according to the provided probability.
            if (!isRoot && (maxDepth == 0 || rand.NextDouble() < leafPropability))
            {
                return RandomLeaf();
            }

            // function node check
            if (rand.NextDouble() < functionPropability)
            {
                string name = functions[rand.Next(functions.Length)];
                MethodInfo method = typeof(Math)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                    .First(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                int argCount = method.GetParameters().Length;
                var args = new List<Node>();
                for (int i = 0; i < argCount; ++i)
                {
                    args.Add(Generate(leafPropability, functionPropability, maxDepth - 1, false));
                }
                return new FunctionNode(name, args.ToArray());

            }

            // Else binary operator node
            Node lhs = Generate(leafPropability, functionPropability, maxDepth - 1, false);
            Node rhs = Generate(leafPropability, functionPropability, maxDepth - 1, false);
            char[] operators = {'+', '-', '*', '/', '%', '^'};
            return new BinaryOperatorNode(Operator.Operators[operators[rand.Next(operators.Length)]], lhs, rhs);
        }

        public static Node RandomLeaf()
        {
            // Occasionally return a number with decimal precision.
            return rand.NextDouble() < .25 
                ? new NumberNode(rand.NextDouble() * 10) 
                : new NumberNode(rand.Next(1, 10));
        }
        
        public abstract class Node
        {
            protected readonly List<Node> Children;

            protected Node()
            {
                Children = new List<Node>();
            }

            protected Node(List<Node> children)
            {
                Children = children;
            }
        }

        public class BinaryOperatorNode : Node
        {
            public readonly Operator Operator;

            public BinaryOperatorNode(Operator op, Node lhs, Node rhs)
                : base(new List<Node>{lhs, rhs})
            {
                Operator = op;
            }

            public override string ToString()
            {
                // None leaf nodes should wrap their contents in parentheses to ensure correct order of operations.
                return $"({Children[0]} {Operator.Symbol} {Children[1]})";
            }
        }
        
        public class FunctionNode : Node
        {
            public readonly string FunctionName;

            public FunctionNode(string functionName, params Node[] arguments)
                : base(arguments.ToList())
            {
                FunctionName = functionName;
            }

            public override string ToString()
            {
                return $"{FunctionName}({string.Join(", ", Children.Select(c => c.ToString()))})";
            }
        }
        
        public class NumberNode : Node
        {
            public readonly double Number;

            public NumberNode(double number)
            {
                Number = number;
            }

            public override string ToString()
            {
                return Number.ToString("0.###", CultureInfo.InvariantCulture);
            }
        }
    }
}