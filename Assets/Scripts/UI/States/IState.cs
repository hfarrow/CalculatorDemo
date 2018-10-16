namespace CalculatorDemo.UI.States
{
    /// <summary>
    /// Interface that when implemented controls what calculator buttons can enabled.
    /// Implementing classes can then track additional state such as number of open
    /// parenthesis or the presence of other expression elements. Correct implementations
    /// of this interface mean the calculator user can never enter invalid characters via
    /// the button GUI.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state is initially activated
        /// </summary>
        void EnterState();
        
        /// <summary>
        /// Called when the sate is terminated
        /// </summary>
        void ExitState();
        
        /// <summary>
        /// Called when the state is temporarily suspended such as when a new state is pushed
        /// ontop of the state being suspended.
        /// </summary>
        void SuspendState();
        
        /// <summary>
        /// Called when the state is resumed such as when a state ontop of the suspended state
        /// is exited.
        /// </summary>
        void ResumeState();
       
        /// <summary>
        /// Should return true if a function (ie: "sin(" ) can be appended to the expression 
        /// </summary>
        /// <returns></returns>
        bool CanAppendFunction();
        
        /// <summary>
        /// Should return true if a binary operator can be appended to the expression 
        /// </summary>
        /// <returns></returns>
        bool CanAppendOperator();
        
        /// <summary>
        /// Should return true if a unary minus can be appended to the expression
        /// Note: If the calculator were to support additional unary operators then this
        /// method should be renamed to CanAppendUnaryOperator and CanAppendOperator should
        /// be renamed to CanAppendBinaryOperator
        /// </summary>
        /// <returns></returns>
        bool CanAppendUnaryMinus();
        
        /// <summary>
        /// Should return true if a decimal can be appended to the expression
        /// Note: This calculator demo supports number that are with or end with a decimal
        ///     such as ".01" or "1."
        /// </summary>
        /// <returns></returns>
        bool CanAppendDecimal();
        
        /// <summary>
        /// Should return true if a number (one or more digits) can be appended to the expression 
        /// </summary>
        /// <returns></returns>
        bool CanAppendNumber();
        
        /// <summary>
        /// Should return true if a parent of the specified type can be appended to the expression 
        /// </summary>
        /// <returns></returns>
        bool CanAppendParen(bool isLeftParen);
        
        /// <summary>
        /// Should return true if a comma can be appended to the expression
        /// Note: This calculator demo only allows a comma when inserting a function.
        /// </summary>
        /// <returns></returns>
        bool CanAppendComma();
        
        /// <summary>
        /// Should return true if expression valid and ready to evaluate.
        /// If all "Can" rules are implemented correctly, there should be no errors when
        /// the expression is evaluated and this function is returning true.
        /// </summary>
        /// <returns></returns>
        bool CanEvaluate();       
    }
}