The calculator features 2 modes, 2 evaluators and tokenizer. The calculator can be run from the scene named "Main".

Evaluators:
    Reverse Polish Notation (RPN) - Uses a Shunting-Yard implementation to parse an infix expression to postfix (RPN)
        expression. The Shunting-Yard implementation is based on sudo code from wikipedia (linked in the source code).
        In addition to the base implementation from wikipedia, my implementation supports unary operators such as
        negation (ie: -5 + 5) and composite functions. All public static methods of System.Math that take numbers as
        arguments are supported.

    Combinator Parser - This algorithm uses a 3rd party combinator parser library called Sprache. It was included to show
        an alternative to the hand written RPN evaluator above. The RPN evaluator does not use any libraries except for
        Unity and C# standard library. This parser demonstrates how little code is required for a numeric expression
        parser. Generally, the performance of a combinator parser is probably worse but they are
        great for rapidly developing DSLs or other prototyping.

Modes:
    Standard - This the default mode and is a GUI only mode. The buttons will enable and disable themselves based on what
        is valid at a given point in the expression. Generally, the disabled buttons guarantee there will be no parsing
        errors during evaluation.
        Bug: The GUI incorrecly allows you to enter "." as a valid number. ".0" and "0." are valid but a lone decimal
        point is not. This could fixed by adjusting the rules to ensure a number is more than just a decimal.
        Alternatively, the parser could interpret "." as ".0".

    Advanced - This mode was used to develop the calculator. You can type the expression directly into the top input field
        and hit enter to evaluate. It shows the results and debugging information of both evaluators. If you intentionally
        evaluate an invalid expression then you should see a semi helpful error message appear. For example, try 
        evaluating an expression with mismatched parenthesis.

Tokenizer:
    The tokenizer uses pure string parsing to walk through an expresion and produce a list of tokens. The tokens are
    then consumed by the Shunting-Yard algorithm and the RPN evaluator. The tokenizer does not allocate strings for
    the substring of the expression a token represents. Instead, the original source string is stored along with the
    index and length of the token.
