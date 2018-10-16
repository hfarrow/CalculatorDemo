using System;
using CalculatorDemo.UI.Controllers;

namespace CalculatorDemo.UI.States
{
    /// <inheritdoc />
    /// <summary>
    /// Serves as a specialization of the BaseState for inserting functions. When inserting
    /// functions, this state must have knowledge of the parameter count of the function and also
    /// must track the number of commas that have already been inserted. Once all arguments have
    /// been appended, only then is it valid to add the function's closing rparen.
    /// </summary>
    public class InsertFunctionState : BaseState
    {
        private readonly int argCount;
        private readonly int expStartIndex;
        private readonly string functionName;
        private int commaCount;

        public InsertFunctionState(StandardCalculatorController mainController, int argCount, int expStartIndex,
            string functionName)
            : base(mainController)
        {
            this.argCount = argCount;
            this.expStartIndex = expStartIndex;
            this.functionName = functionName;
        }
        
        public override void EnterState()
        {
            base.EnterState();
            AddListeners();
            
            commaCount = 0;
            if (MainController.InputField.text.Length > expStartIndex + 1 + functionName.Length + 1)
            {
                // Assume we are returning to this state via "undo" and that the cursor is at where the closing rparen
                // used to be.
                commaCount = argCount - 1;
            }
        }

        public override void ExitState()
        {
            base.ExitState();
            RemoveListeners();
        }

        public override void SuspendState()
        {
            base.SuspendState();
            RemoveListeners();
        }

        public override void ResumeState()
        {
            base.ResumeState();
            AddListeners();
        }

        private void AddListeners()
        {
            MainController.CommaButton.gameObject.SetActive(true);
            MainController.OnAppend += OnAppended;
        }

        private void RemoveListeners()
        {
            MainController.OnAppend -= OnAppended;
        }

        private void OnAppended(string str)
        {
            switch (str)
            {
                case ")":
                    if (ParenDepth < 0)
                    {
                        // The function's closing rparen was just appended so it's time to
                        // exit this state.
                        MainController.PopState();
                    }

                    break;
                case ",":
                    // Only count the number of arguments added for this function.
                    // Without this check, the commas of nested (composite) functions would
                    // also be counted which is incorrect.
                    if (ParenDepth == 0)
                    {
                        ++commaCount;
                    }
                    break;
            }
        }

        public override bool CanAppendParen(bool isLeftParen)
        {
            if (isLeftParen)
            {
                return base.CanAppendParen(true);
            }
            
            char prev = MainController.PreviousChar;
            return prev != ',' && base.CanAppendParen(false) && !(ParenDepth == 0 && CanAppendComma());
        }

        public override bool CanAppendComma()
        {
            char prev = MainController.PreviousChar;
            return commaCount < argCount - 1 && prev != '(' && !Operator.Operators.ContainsKey(prev) && ParenDepth <= 0;
        }

        public override bool CanEvaluate()
        {
            // It is never possible to evaluate while this state is active. When the closing
            // rparen is added, the state is immediately exited and new active state gets
            // to decide.
            return false;
        }
    }
}