using System;
using System.Globalization;
using System.Linq.Expressions;
using CalculatorDemo.Algorithms;
using CalculatorDemo.Evaluators;
using UnityEngine;
using UnityEngine.UI;

namespace CalculatorDemo.UI.Controllers
{
    /// <summary>
    /// This controller was used during development. It useful if a user wishes to paste an
    /// expression into the calculator or if they prefer to manually type the expression without
    /// the safety of the using GUI buttons.
    /// </summary>
    public class AdvancedCalculatorController : MonoBehaviour
    {
        public InputField InputField;
        public InputField RpnField;
        public InputField RpnExpressionTreeField;
        public InputField CombinatorExpressionTreeField;
        public InputField RpnResultField;
        public InputField CombinatorResultField;
        public InputField RpnErrorsField;
        public InputField CombinatorErrorsField;

        private ReversePolishNotationEvaluator rpnEvaluator;
        private CombinatorParserEvaluator combinatorEvaluator;

        private void Awake()
        {
            rpnEvaluator = new ReversePolishNotationEvaluator();
            combinatorEvaluator = new CombinatorParserEvaluator();
        }

        private void Start()
        {
            InputField.ActivateInputField();
        }

        /// <summary>
        /// Called from button wired through the Unity inspector
        /// </summary>
        public void Evaluate()
        {
            Result<string> shuntingYardResult = ShuntingYard.InfixToPostfixStr(InputField.text);
            RpnField.text = shuntingYardResult.HasValue
                ? shuntingYardResult.Value
                : "<error>";
            
            Result<Expression<Func<double>>> rpnExpTreeResult =
                rpnEvaluator.TryEvaluateToExpressionTree(InputField.text);
            RpnExpressionTreeField.text = rpnExpTreeResult.HasValue
                ? rpnExpTreeResult.Value.ToString()
                : "<error>";
            
            Result<Expression<Func<double>>> combinatorExpTreeResult =
                combinatorEvaluator.TryEvaluateToExpressionTree(InputField.text);
            CombinatorExpressionTreeField.text = combinatorExpTreeResult.HasValue
                ? combinatorExpTreeResult.Value.ToString()
                : "<error>";

            
            Result<double> rpnResult = rpnEvaluator.TryEvaluate(InputField.text);
            if (!rpnResult.HasValue)
            {
                RpnErrorsField.text = rpnResult.ErrorMessage;
                RpnResultField.text = "<error>";
            }
            else
            {
                RpnErrorsField.text = string.Empty;
                RpnResultField.text = rpnResult.Value.ToString(CultureInfo.InvariantCulture);
            }
            
            Result<double> combinatorResult = combinatorEvaluator.TryEvaluate(InputField.text);
            if (!combinatorResult.HasValue)
            {
                CombinatorErrorsField.text = combinatorResult.ErrorMessage;
                CombinatorResultField.text = "<error>";
            }
            else
            {
                CombinatorErrorsField.text = string.Empty;
                CombinatorResultField.text = combinatorResult.Value.ToString(CultureInfo.InvariantCulture);
            }
            
            InputField.ActivateInputField();
        }

        /// <summary>
        /// Called from button wired through the Unity inspector
        /// </summary>
        public void EvaluateRandomExpression()
        {
            InputField.text = ExpressionGenerator.Generate(0.5, 0.3, 3).ToString();
            Evaluate();
        }
    }
}