using System.Linq;
using CalculatorDemo.UI.Controllers;

namespace CalculatorDemo.UI.States
{
    /// <inheritdoc />
    /// <summary>
    /// Serves as the default (lowest state) for a calculator. From this state, a function
    /// can not be added. To insert a function, the InsertFunctionState should be pushed ontop
    /// of a state stack.
    /// </summary>
    public class BaseState : IState
    {
        protected readonly StandardCalculatorController MainController;
        
        /// <summary>
        /// Tracks the current perenthesis depth even while the state is suspended.
        /// </summary>
        protected int ParenDepth { private set; get; }

        public BaseState(StandardCalculatorController mainController)
        {
            MainController = mainController;
        }
        
        public virtual void EnterState()
        {
            ParenDepth = 0;
            MainController.CommaButton.gameObject.SetActive(false);
            MainController.OnAppend += OnAppend;
            MainController.OnUndo += OnUndo;
        }

        public virtual void ExitState()
        {
            // Continue to count parens even while state is suspended. Only remove the listener
            // when exiting.
            MainController.OnAppend -= OnAppend;
            MainController.OnUndo -= OnUndo;
        }

        public virtual void SuspendState()
        {
        }

        public virtual void ResumeState()
        {
            MainController.CommaButton.gameObject.SetActive(false);
        }

        private void OnAppend(string str)
        {
            ParenDepth += str.Count(c => c == '(');
            ParenDepth -= str.Count(c => c == ')');
        }

        private void OnUndo(string str)
        {
            ParenDepth -= str.Count(c => c == '(');
            ParenDepth += str.Count(c => c == ')');
        }

        public virtual bool CanAppendFunction()
        {
            char prev = MainController.PreviousChar;
            return Operator.Operators.ContainsKey(prev) || prev == '(' || prev == ',' || prev == ' ';
        }

        public virtual bool CanAppendOperator()
        {
            char prev = MainController.PreviousChar;
            return (char.IsNumber(prev) || prev == '.' || prev == ')') && prev != ' ';
        }

        public virtual bool CanAppendDecimal()
        {
            char prev = MainController.PreviousChar;
            if (prev == ' ' || Operator.Operators.ContainsKey(prev) || prev == '(')
            {
                return true;
            }

            if (prev == ')' || prev == '.')
            {
                return false;
            }
            
            // When the previous character is a digit, walk backwards through the expression
            // and see if a decimal was already added to the current number.
            // Note: Alternatively, an "InsertNumberState" could be created and track when the first
            // decimal is added and return false for it's implementation of CanAppendDecimal.
            if (char.IsDigit(prev))
            {
                string expression = MainController.InputField.text;
                for (int i = expression.Length - 1; i >= 0; --i)
                {
                    if (!char.IsDigit(expression[i]))
                    {
                        return expression[i] != '.';
                    }
                }
            }

            // All cases should be handled above but if one is missed, error on letting the user
            // input an invalid character as opposed to being unable to insert a decimal where it
            // should be allowed.
            return true;
        }

        public virtual bool CanAppendNumber()
        {
            char prev = MainController.PreviousChar;
            return char.IsDigit(prev) || prev == ' ' || prev == '.' || Operator.Operators.ContainsKey(prev) 
                   || prev == '(' || prev == ',';
        }

        public virtual bool CanAppendParen(bool isLeftParen)
        {
            char prev = MainController.PreviousChar;
            if (isLeftParen)
            {
                return prev == '(' || prev == ',' || prev == ' ' ||
                       Operator.Operators.ContainsKey(prev) || prev == ' ';
            }
            
            return MainController.BaseState.ParenDepth > 0 && (char.IsDigit(prev) || prev == '.' || prev == ')');
        }

        public virtual bool CanAppendComma()
        {
            return false;
        }

        public virtual bool CanEvaluate()
        {
            return CanAppendOperator() && ParenDepth == 0;
        }

        public bool CanAppendUnaryMinus()
        {
            char prev = MainController.PreviousChar;
            bool canAppendNumber = CanAppendNumber();
            return canAppendNumber && !char.IsDigit(prev) && prev != '.';
        }
    }
}