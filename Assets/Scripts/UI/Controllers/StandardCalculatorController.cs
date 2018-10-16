using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CalculatorDemo.Evaluators;
using CalculatorDemo.UI.States;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace CalculatorDemo.UI.Controllers
{
    /// <summary>
    /// A controller for a calculator with a full set of GUI buttons for all functionality. This controller offers
    /// the user safety. It should not be possible to enter an invalid character at any point.
    /// 
    /// The GUI buttons are disabled when the button would append invalid elements to the expression. This is
    /// accomplished by using a pushdown automaton. A stack of controller states are maintained and the top state
    /// determined what buttons should be enabled after the expression receives an addition. More specifically, this
    /// controller polls the top state to ask it what buttons are valid.
    /// 
    /// Only two types of states are required for the standard calculator. One is a base state which is the default
    /// state for an empty expression. The second state is a specialization of the base state and it is named
    /// "InsertFunctionState". The insert function state must track how many arguments are expected and how many arguments
    /// have been ented. The insert function state is popped as soon as the closing right parentheses is appended.
    /// </summary>
    public class StandardCalculatorController : MonoBehaviour
    {
        public InputField InputField;
        public GameObject ButtonContainer;
        public InputField ErrorField;
        public InputField DebugField;
        public Button CommaButton => buttonObjects["comma"].GetComponent<Button>();
        
        public event Action<string> OnAppend;
        public event Action<string> OnUndo;
        
        public char PreviousChar
        {
            get
            {
                if (undoStack.Count == 0)
                {
                    return ' ';
                }
                string previousAppend = undoStack.Peek().ExpressionPart;
                return previousAppend[previousAppend.Length - 1];
            }
        }

        private Stack<IState> stateStack = new Stack<IState>();
        private IState CurrentState => stateStack.Count > 0 ? stateStack.Peek() : null;
        private readonly Stack<StateHistory> undoStack = new Stack<StateHistory>();
        private Dictionary<string, UnityAction> buttonActions;
        private readonly Dictionary<string, GameObject> buttonObjects = new Dictionary<string, GameObject>();
        private IEvaluator evaluator = new ReversePolishNotationEvaluator();
        public BaseState BaseState { private set; get; }

        private readonly string[] functionNames =
        {
            "sqrt",
            "floor",
            "ceil",
            "min",
            "max",
            "abs",
            "sin",
            "cos",
            "tan",
            "atan",
            "atan2",
        };

        private readonly string[] operatorNames =
        {
            "add",
            "subtract",
            "multiply",
            "divide",
            "mod",
            "pow",
        };

        private readonly string[] numberNames =
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };


        private void Awake()
        {
            Assert.IsNotNull(InputField, $"{nameof(InputField)} cannot be null");
            Assert.IsNotNull(ButtonContainer, $"{nameof(ButtonContainer)} cannot be null");
            Assert.IsNotNull(ErrorField, $"{nameof(ErrorField)} cannot be null");
            Assert.IsNotNull(DebugField, $"{nameof(DebugField)} cannot be null");
            
            buttonActions = new Dictionary<string, UnityAction>
            {
                ["clear"] = Clear,
                ["evaluate"] = Evaluate,
                ["add"] = () => AppendOperator('+'),
                ["subtract"] = () => AppendOperator('-'),
                ["multiply"] = () => AppendOperator('*'),
                ["divide"] = () => AppendOperator('/'),
                ["mod"] = () => AppendOperator('%'),
                ["pow"] = () => AppendOperator('^'),
                ["decimal"] = AppendDecimal,
                ["0"] = () => AppendNumber('0'),
                ["1"] = () => AppendNumber('1'),
                ["2"] = () => AppendNumber('2'),
                ["3"] = () => AppendNumber('3'),
                ["4"] = () => AppendNumber('4'),
                ["5"] = () => AppendNumber('5'),
                ["6"] = () => AppendNumber('6'),
                ["7"] = () => AppendNumber('7'),
                ["8"] = () => AppendNumber('8'),
                ["9"] = () => AppendNumber('9'),
                ["sqrt"] = () => AppendFunction("sqrt", 1),
                ["floor"] = () => AppendFunction("floor", 1),
                ["ceil"] = () => AppendFunction("ceil", 1),
                ["min"] = () => AppendFunction("min", 2),
                ["max"] = () => AppendFunction("max", 2),
                ["abs"] = () => AppendFunction("abs", 1),
                ["sin"] = () => AppendFunction("sin", 1),
                ["cos"] = () => AppendFunction("cos", 1),
                ["tan"] = () => AppendFunction("tan", 1),
                ["atan"] = () => AppendFunction("atan", 1),
                ["atan2"] = () => AppendFunction("atan2", 2),
                ["lparen"] = () => AppendParen('('),
                ["rparen"] = () => AppendParen(')'),
                ["comma"] = AppendComma,
            };
        }

        private void Start()
        {
            foreach (KeyValuePair<string, UnityAction> kvp in buttonActions)
            {
                GameObject buttonObj = transform.Search(kvp.Key)?.gameObject;
                Button button = buttonObj?.GetComponent<Button>();
                if (buttonObj == null)
                {
                    Debug.LogErrorFormat("Failed to find required button with name '{0}'", kvp.Key);
                }
                else if (button == null)
                {
                    Debug.LogErrorFormat("Failed to find button component on button with name '{0}'", kvp.Key);
                }
                else
                {
                    button.onClick.AddListener(kvp.Value);
                    buttonObjects[kvp.Key] = button.gameObject;
                }
            }
            
            CommaButton.gameObject.SetActive(false);
            Clear();
        }

        public void PushState(IState state)
        {
            CurrentState?.SuspendState();
            stateStack.Push(state);
            CurrentState.EnterState();
            SetDebugText();
        }

        public void PopState(bool isUndo = false)
        {
            CurrentState.ExitState();
            IState popped = stateStack.Pop();
            if (!isUndo && undoStack.Count > 0)
            {
                undoStack.Peek().PopState = popped;
            }
            CurrentState?.ResumeState();
            SetDebugText();
        }

        public void Append(string str)
        {
            Append(str, new StateHistory(str));
        }
        
        public void Append(string str, StateHistory history)
        {
            undoStack.Push(history);
            OnAppend?.Invoke(str);
            if (history.PushState != null)
            {
                PushState(history.PushState);
            }
            InputField.text += str;
            DisableInvalidButtons();
        }
        
        public void AppendPushState(string str, IState newState)
        {
            Append(str, new StateHistory(str, pushState: newState));
        }
        
        public void Append(char c)
        {
            Append(c.ToString());
        }

        private void DisableInvalidButtons()
        {
            ToggleButtonGroup(CurrentState.CanAppendFunction(), functionNames);
            ToggleButtonGroup(CurrentState.CanAppendOperator(), operatorNames);
            ToggleButtonGroup(CurrentState.CanAppendNumber(), numberNames);
            ToggleButtonGroup(CurrentState.CanAppendDecimal(), "decimal");
            ToggleButtonGroup(CurrentState.CanAppendParen(true), "lparen");
            ToggleButtonGroup(CurrentState.CanAppendParen(false), "rparen");
            ToggleButtonGroup(CurrentState.CanAppendComma(), "comma");
            ToggleButtonGroup(CurrentState.CanAppendUnaryMinus() || CurrentState.CanAppendOperator(), "subtract");
            ToggleButtonGroup(CurrentState.CanEvaluate(), "evaluate");
        }

        private void ToggleButtonGroup(bool isEnabled, params string[] buttonNames)
        {
            foreach (string buttonName in buttonNames)
            {
                GameObject buttonObj;
                buttonObjects.TryGetValue(buttonName, out buttonObj);

                if (buttonObj == null)
                {
                    Debug.LogErrorFormat("Failed to find button '{0}'", buttonName);
                }
                else
                {
                    var button = buttonObj.GetComponent<Button>();
                    button.interactable = isEnabled;
                }
            }
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                string currentExp = InputField.text;
                StateHistory popped = undoStack.Pop();
                InputField.text = string.Join("", undoStack.Reverse().Select(s => s.ExpressionPart));

                OnUndo?.Invoke(currentExp.Substring(InputField.text.Length));
                
                if (popped.PushState != null)
                {
                    PopState(true);
                }
                if (popped.PopState != null)
                {
                    PushState(popped.PopState);
                }

                DisableInvalidButtons();
                SetDebugText();
            }
        }

        public void Evaluate()
        {
            Result<Expression<Func<double>>> expTree = null;
            string exMsg = null;
            try
            {
                expTree = evaluator.TryEvaluateToExpressionTree(InputField.text);
            }
            catch (Exception e)
            {
                exMsg = e.Message;
            }

            if (expTree == null || !expTree.HasValue)
            {
                ErrorField.gameObject.SetActive(true);
                ErrorField.text = expTree?.ErrorMessage ?? exMsg;
            }
            else
            {
                ErrorField.gameObject.SetActive(false);
                string result = expTree.Value.Compile()().ToString(CultureInfo.InvariantCulture);
                Clear();
                
                // Carry over the result
                Append(result);
            }
        }

        public void SetRandomExpression()
        {
            Clear();
            Append(ExpressionGenerator.Generate(0.5, 0.3, 3).ToString());
            DisableInvalidButtons();
        }

        public void Clear()
        {
            InputField.text = string.Empty;
            ErrorField.gameObject.SetActive(false);
            ErrorField.text = string.Empty;
            while (stateStack.Count > 0)
            {
                PopState();
            }
            undoStack.Clear();
            stateStack.Clear();
            BaseState = new BaseState(this);
            PushState(BaseState);
            DisableInvalidButtons();
        }

        private void AppendFunction(string functionName, int argCount)
        {
            if (CurrentState.CanAppendFunction())
            {
                AppendPushState($"{functionName}(",
                    new InsertFunctionState(this, argCount, InputField.text.Length - 1, functionName));
            }
        }

        private void AppendOperator(char opChar)
        {
            if (CurrentState.CanAppendOperator() || (opChar == '-' && CurrentState.CanAppendUnaryMinus()))
            {
                Operator op = Operator.Operators[opChar];
                Append(op.Symbol);
            }
        }

        private void AppendDecimal()
        {
            if (CurrentState.CanAppendDecimal())
            {
                Append(".");
            }
        }

        private void AppendNumber(char numberChar)
        {
            if (CurrentState.CanAppendNumber())
            {
                Append(numberChar);
            }
        }

        private void AppendParen(char parenChar)
        {
            if (CurrentState.CanAppendParen(parenChar == '('))
            {
                Append(parenChar);
            }
        }
        
        private void AppendComma()
        {
            if (CurrentState.CanAppendComma())
            {
                Append(',');
            }
        }

        private void SetDebugText()
        {
            var msg = new StringBuilder();
            msg.AppendLine("State Stack: " + string.Join(" > ", stateStack.Reverse()));
            msg.AppendLine();
            msg.AppendLine("Undo Stack: " + string.Join(" > ",
                               undoStack.Reverse().Select(s =>
                                   string.Format("{0}|push:{1}|pop:{2}",
                                       s.ExpressionPart,
                                       s.PushState?.GetType().Name,
                                       s.PopState?.GetType().Name))));
            DebugField.text = msg.ToString();
        }
    }

    public class StateHistory
    {
        public readonly string ExpressionPart;
        public readonly IState PushState;
        public IState PopState;


        public StateHistory(string expressionPart, 
            IState pushState = null, 
            IState popState = null)
        {
            ExpressionPart = expressionPart;
            PushState = pushState;
            PopState = popState;
        }
    }
}