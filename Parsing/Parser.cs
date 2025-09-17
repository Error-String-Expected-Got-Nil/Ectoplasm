using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Parsing.Expressions.BinaryOperators;
using Ectoplasm.Parsing.Expressions.UnaryOperators;
using Ectoplasm.Parsing.Statements;
using Ectoplasm.Runtime.Values;

using static Ectoplasm.Lexing.TokenType;

namespace Ectoplasm.Parsing;

/// <summary>
/// Static class holding code for parsing of lexically analyzed Lua source code.
/// </summary>
public static class Parser
{
    /// <summary>
    /// Parse a set of <see cref="LuaToken"/>s as a single chunk of Lua code.
    /// </summary>
    /// <param name="source">Array of <see cref="LuaToken"/>s. Should not contain any tokens of type
    /// <see cref="TokenType.Whitespace"/> or <see cref="TokenType.Comment"/>, discard them before providing a sequence
    /// to the parser. The sequence should also end with an <see cref="TokenType.EndOfChunk"/> token.
    /// </param>
    /// <returns>[FILL IN DOCUMENTATION]</returns>
    /// <exception cref="LuaParsingException">
    /// [FILL IN DOCUMENTATION]
    /// </exception>
    public static ParsedChunk Parse(LuaToken[] source)
    {
        return null!;
    }

    /// <summary>
    /// Parse a sequence of <see cref="LuaToken"/>s as a block of statements, starting from the given position.
    /// </summary>
    /// <returns>The list of statements that make up the block, and the number of tokens that make them up.</returns>
    public static (List<Statement> Block, int Length) ParseBlock(LuaToken[] source, int position)
    {
        var statements = new List<Statement>();

        var offset = 0;
        while (true)
        {
            var token = source[position + offset];

            switch (token.Type)
            {
                case TokenType.Statement:
                    statements.Add(new Stat_Empty(token.StartLine, token.StartCol));
                    offset++;
                    continue;
                
                case LabelSep:
                    var labelName = source[position + offset + 1];
                    
                    if (labelName.Type is not Name)
                        throw new LuaParsingException(labelName, "expected Name of label");

                    var labelEndToken = source[position + offset + 2];
                    if (labelEndToken.Type is not LabelSep)
                        throw new LuaParsingException(labelEndToken, "expected closing label separator");
                    
                    statements.Add(new Stat_Label((string)labelName.Data!, token.StartLine, token.StartCol));
                    offset += 3;
                    continue;
                
                case Goto:
                    var targetLabelName = source[position + offset + 1];

                    if (targetLabelName.Type is not Name)
                        throw new LuaParsingException(targetLabelName, "expected Name of target label");
                    
                    statements.Add(new Stat_Goto((string)targetLabelName.Data!, token.StartLine, token.StartCol));
                    offset += 2;
                    continue;
                
                case Break:
                    statements.Add(new Stat_Break(token.StartLine, token.StartCol));
                    offset++;
                    continue;
                
                case Do:
                    var (doBlockContents, doBlockLength) = ParseBlock(source, position + offset + 1);

                    var doBlockEndToken = source[position + offset + 1 + doBlockLength];
                    if (doBlockEndToken.Type is not End)
                        throw new LuaParsingException(doBlockEndToken, 
                            $"expected end to do block started on line {token.StartLine}, column {token.StartCol}");
                    
                    statements.Add(new Stat_Do(doBlockContents, token.StartLine, token.StartCol));
                    offset += 1 + doBlockLength + 1;
                    continue;
                
                case While:
                    var (whileExp, whileExpLength) = ParseExpression(source, position + offset + 1);

                    var whileDoToken = source[position + offset + 1 + whileExpLength];
                    if (whileDoToken.Type is not Do)
                        throw new LuaParsingException(whileDoToken,
                            "expected 'do' to delimit expression of while loop started on line " +
                            $"{token.StartLine}, column {token.StartCol}");

                    var (whileBlockContents, whileBlockLength) =
                        ParseBlock(source, position + offset + 1 + whileExpLength + 1);

                    var whileEndToken = source[position + offset + 1 + whileExpLength + 1 + whileBlockLength + 1];
                    if (whileEndToken.Type is not End)
                        throw new LuaParsingException(whileEndToken,
                            "expected end to while statement do block started on " +
                            $"line {whileDoToken.StartLine}, column {whileDoToken.StartCol}");

                    statements.Add(new Stat_While(whileExp, whileBlockContents, token.StartLine, token.StartCol));
                    offset += 1 + whileExpLength + 1 + whileBlockLength + 1;
                    continue;
                
                case Repeat:
                    var (repeatBlockContents, repeatBlockLength) = ParseBlock(source, position + offset + 1);

                    var repeatUntilToken = source[position + offset + 1 + repeatBlockLength];
                    if (repeatUntilToken.Type is not Until)
                        throw new LuaParsingException(repeatUntilToken,
                            "expected 'until' to delimit block of repeat statement started on " +
                            $"line {token.StartLine}, column {token.StartCol}");

                    var (untilExp, untilExpLength) =
                        ParseExpression(source, position + offset + 1 + repeatBlockLength + 1);

                    statements.Add(new Stat_Repeat(untilExp, repeatBlockContents, token.StartLine, token.StartCol));
                    offset += 1 + repeatBlockLength + 1 + untilExpLength;
                    continue;
                
                case If:
                    var (ifExp, ifExpLength) = ParseExpression(source, position + offset + 1);

                    var ifThenToken = source[position + offset + 1 + ifExpLength];
                    if (ifThenToken.Type is not Then)
                        throw new LuaParsingException(ifThenToken,
                            "expected 'then' to delimit expression of if statement started on " +
                            $"line {token.StartLine}, column {token.StartCol}");

                    var (ifBlockContents, ifBlockLength) = ParseBlock(source, position + offset + 1 + ifExpLength + 1);

                    var ifClauses = new List<(Expression Condition, List<Statement> Block)> 
                        { (ifExp, ifBlockContents) };
                    List<Statement>? elseBlock = null;
                    
                    offset += 1 + ifExpLength + 1 + ifBlockLength;
                    while (true)
                    {
                        var ifContinuationToken = source[position + offset];
                        
                        if (ifContinuationToken.Type is Else)
                        {
                            var (elseBlockContents, elseBlockLength) = ParseBlock(source, position + offset + 1);
                            elseBlock = elseBlockContents;
                            offset += 1 + elseBlockLength;
                            break;
                        }

                        if (ifContinuationToken.Type is Elseif)
                        {
                            var (elseifExp, elseifExpLength) = ParseExpression(source, position + offset + 1);

                            var elseifThenToken = source[position + offset + 1 + elseifExpLength];
                            if (elseifThenToken.Type is not Then)
                                throw new LuaParsingException(elseifThenToken,
                                    "expected 'then' to delimit expression of elseif clause started on " +
                                    $"line {ifContinuationToken.StartLine}, column {ifContinuationToken.StartCol}");

                            var (elseifBlockContents, elseifBlockLength) =
                                ParseBlock(source, position + offset + 1 + elseifExpLength + 1);
                            
                            ifClauses.Add((elseifExp, elseifBlockContents));
                            offset += 1 + elseifExpLength + 1 + elseifBlockLength;
                            continue;
                        }

                        break;
                    }

                    var ifEndToken = source[position + offset];
                    if (ifEndToken.Type is not End)
                        throw new LuaParsingException(ifEndToken, "expected end to if statement started on " +
                                                                  $"line {token.StartLine}, column {token.StartCol}");
                    
                    statements.Add(new Stat_If(ifClauses, elseBlock, token.StartLine, token.StartCol));
                    offset++;
                    continue;
                
                case For:
                    var (namelist, namelistLength) = ParseNamelist(source, position + offset + 1);

                    var followingToken = source[position + offset + 1 + namelistLength];
                    offset += 1 + namelistLength + 1;
                    
                    if (followingToken.Type is Assign)
                    {
                        // For statement starting with initial assignment must be followed by a separator, then an
                        // expression, then optionally another separator and expression.

                        if (namelist.Count != 1)
                            throw new LuaParsingException("For loop with initial assignment may only define one name",
                                token.StartLine, token.StartCol);
                        
                        var (initExp, initExpLength) = ParseExpression(source, position + offset);

                        // Separator between assignment and end expression
                        var initEndSep = source[position + offset + initExpLength];

                        if (initEndSep.Type is not Separator)
                            throw new LuaParsingException(initEndSep, 
                                "expected separator between assignment and end expressions of for statement " +
                                $"started on line {token.StartLine}, column {token.StartCol}");

                        offset += initExpLength + 1;
                        var (endExp, endExpLength) = ParseExpression(source, position + offset);

                        // Token following the end expression. If it is a separator, we parse the increment expression
                        // after it, then set it to the token following that expression. Otherwise, we continue to
                        // verifying it's a 'do' token.
                        offset += endExpLength;
                        var forContinuationToken = source[position + offset];

                        Expr_Root? incExp = null;
                        if (forContinuationToken.Type is Separator)
                        {
                            (incExp, var incExpLength) = ParseExpression(source, position + offset);
                            
                            offset += incExpLength;
                            forContinuationToken = source[position + offset];
                        }

                        if (forContinuationToken.Type is not Do)
                            throw new LuaParsingException(forContinuationToken,
                                "expected 'do' to delimit header of for statement started on line " +
                                $"{token.StartLine}, column {token.StartCol}");

                        offset++;
                        var (forBlockContents, forBlockLength) = ParseBlock(source, position + offset);
                        offset += forBlockLength;

                        var forEndToken = source[position + offset];
                        if (forEndToken.Type is not End)
                            throw new LuaParsingException(forEndToken,
                                $"expected end to for loop started on line {token.StartLine}, " +
                                $"column {token.StartCol}");

                        offset++;
                        statements.Add(new Stat_ForAssign(namelist[0], initExp, endExp, incExp, 
                            forBlockContents, token.StartLine, token.StartCol));
                        continue;
                    }
                    
                    if (followingToken.Type is In)
                    {
                        // TODO
                    }
                    
                    // Following token was not an Assign or In token
                    throw new LuaParsingException(followingToken, 
                        "expected 'in' or '=' after name or namelist at beginning of for statement started on " + 
                        $"line {token.StartLine}, column {token.StartCol}");
                
                // TODO: Local variable definitions, function definitions, local function definitions,
                //  function call statements, assignment statements
            }
        }

        return (statements, offset);
    }

    private static (List<LuaToken> Names, int Length) ParseNamelist(LuaToken[] source, int position)
    {
        var names = new List<LuaToken>();

        var offset = 0;
        while (true)
        {
            var token = source[position + offset];

            if (token.Type is not Name)
                throw new LuaParsingException(token, $"expected Name in namelist, got {token.Type} instead");
            
            names.Add(token);

            offset++;
            token = source[position + offset];

            // Check for separator between names. If not there, we've reached the end of the namelist.
            if (token.Type is not Separator)
                return (names, offset); // Offset already 1 more than number of tokens consumed

            // Otherwise, increment offset to skip the separator.
            offset++;
        }
    }
    
    /// <summary>
    /// Parses a Lua expression starting at a given position in a sequence of source tokens. 
    /// </summary>
    /// <param name="source">Source token sequence to parse.</param>
    /// <param name="position">Index in source to start parsing at.</param>
    /// <param name="terminateOnDelimiter">
    /// On encountering an unexpected delimiter token, the parser will simply stop parsing instead of throwing an error.
    /// Only applies to closing parentheses, closing brackets, and closing curly brackets. This is used for recursive
    /// calls.
    /// </param>
    /// <param name="allowZeroLength">
    /// If true, an exception will not be thrown if the expression is parsed as zero-length. In this case, the returned
    /// expression will be null, and the returned length will be 0.
    /// </param>
    /// <returns>The parsed expression, and the number of tokens that make it up.</returns>
    // This is a variant of Dijkstra's shunting yard algorithm adapted for Lua's syntax, and which is able to validate
    // expressions as it parses them.
    public static (Expr_Root Expr, int Length) ParseExpression(LuaToken[] source, int position, 
        bool terminateOnDelimiter = false, bool allowZeroLength = false)
    {
        if (position >= source.Length)
            throw new LuaParsingException("Attempt to parse expression starting after end of source");

        var startLine = source[position].StartLine;
        var startCol = source[position].StartCol;
        
        // Flag indicating if we expect the next token to be a value, rather than an operator or some other symbol.
        // Used to both validate expressions and distinguish unary operators.
        var expectingValue = true;

        // Flag indicating if the last parsed portion of the expression was a prefix expression per Lua's syntax.
        // That is: A Name, an index operation, a function call, or a parenthesized expression.
        var lastWasPrefixExp = false;

        var output = new Stack<Expression>();
        var operatorStack = new Stack<LuaToken>();

        var offset = 0;
        
        // Loop is terminated by encountering token that cannot be part of an expression. This will always happen due to
        // the EndOfChunk token.
        while (true)
        {
            var token = source[position + offset];

            // Closing index or table is always invalid mid-expression, unless this was a recursive call, in which case
            // we might be parsing an expression inside a table or index, so we just break.
            if (token.Type is CloseIndex or CloseTable)
            {
                if (terminateOnDelimiter) break;
                throw new LuaParsingException(token);
            }
            
            if (IsValue(token))
            {
                if (!expectingValue)
                {
                    if (lastWasPrefixExp && token.Type is TokenType.String or OpenTable)
                    {
                        ParseLiteralCall();
                        // expectingValue and lastWasPrefixExp remain the same after this
                        continue;
                    }

                    // If we see a value when not expecting one, and it wasn't a literal argument to a function call,
                    // then it's the next statement, the expression is finished.
                    break;
                }
                
                // Otherwise, we push it to the output stack immediately.
                output.Push(token.Type switch
                {
                    Name => new Expr_Variable((string)token.Data!, token.StartLine, token.StartCol),
                    TokenType.String => new Expr_String((string)token.Data!, token.StartLine, token.StartCol),
                    Varargs => new Expr_Varargs(token.StartLine, token.StartCol),
                    _ => new Expr_Value(new LuaValue(token), token.StartLine, token.StartCol)
                });

                expectingValue = false;
                lastWasPrefixExp = token.Type == Name;

                offset++;
                continue;
            }

            if (token.Type is Function)
            {
                // If we find a function declaration, and we aren't expecting a value, that's the next statement, we can
                // end parsing
                if (!expectingValue) break;
                
                // Otherwise, parse the function as a value
                // TODO: Parse function def
                throw new NotImplementedException();
            }

            if (token.Type is OpenExp)
            {
                // Interpretation of opening parenthesis depends on if we were expecting a value or not. If we were,
                // we can treat it like a value where we still expect a value afterward. If not, it's a function call.
                if (expectingValue)
                {
                    operatorStack.Push(token);
                    lastWasPrefixExp = false;
                    offset++;
                    continue;
                }

                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, "function call must be preceded by a Name, index " +
                                                         "operation, function call, or parenthesized expression");

                ParseCall(); // ParseCall pushes the call expression itself
                offset++; // Offset will be on closing parenthesis after ParseCall(), need to increment
                continue;
            }
            
            if (token.Type is OpenTable)
            {
                var (table, length) = ParseTableConstructor(source, position + offset);
                output.Push(table);
                expectingValue = false;
                lastWasPrefixExp = false;
                offset += length;
                continue;
            }
            
            // At this point, we know we're parsing an operator of some kind. If we were expecting a value, this is only
            // valid if it's a unary operator.
            if (expectingValue)
            {
                if (!Grammar.UnaryOperators.Contains(token.Type))
                    throw new LuaParsingException(token, "expected value or unary operator token");

                // For tokens that could be either unary or binary, replace the binary token with its unary counterpart
                if (token.Type is Sub)
                    token = token with { Type = Neg };
                else if (token.Type is BitwiseXor)
                    token = token with { Type = BitwiseNot };
            }
            
            if (token.Type is IndexName)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, "index operator must be preceded by a Name, index " +
                                                         "operation, function call, or parenthesized expression");

                var next = source[position + offset + 1];
                
                if (next.Type is not Name)
                    throw new LuaParsingException(next, "expected Name operand for index operator");
                
                // Index operations are evaluated without precedence, we simply push them immediately to the output
                // For the dot indexing form, the "Name" operand is actually syntactic sugar for a string literal
                output.Push(new Expr_String((string)next.Data!, next.StartLine, next.StartCol));
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                offset += 2;
                continue;
            }

            if (token.Type is IndexMethod)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, "method operator must be preceded by a Name, index " +
                                                         "operation, function call, or parenthesized expression");

                var next = source[position + offset + 1];
                
                if (next.Type is not Name)
                    throw new LuaParsingException(next, "expected Name operand for method operator");

                output.Push(new Expr_String((string)next.Data!, next.StartLine, next.StartCol));
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                offset += 2;
                
                // Source will always end with an EndOfChunk token, so we know there will always be a second-next token
                // if the next token wasn't an EndOfChunk (and we currently know it's a Name)
                var argsStart = source[position + offset];
                
                if (argsStart.Type is TokenType.String or OpenTable)
                {
                    ParseLiteralCall(true);
                    continue;
                }
                
                if (argsStart.Type is not OpenExp)
                    throw new LuaParsingException(argsStart, "expected arguments for method operator");
                
                ParseCall(true); // Parse call pushes the call expression itself
                offset++;
                continue;
            }

            if (token.Type is OpenIndex)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, "index operator must be preceded by a Name, index " +
                                                         "operation, function call, or parenthesized expression");

                // Index operation in brackets contains an expression, can recursively parse
                var (operand, length) = ParseExpression(source, position + offset + 1, 
                    true);
                
                offset += length + 1;
                var close = source[position + offset];
                if (close.Type is not CloseIndex)
                    throw new LuaParsingException(close, "expected close to index operation on line " +
                                                         $"{token.StartLine}, column {token.StartCol}");
                
                output.Push(operand);
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                offset++;
                continue;
            }
            
            if (token.Type is CloseExp)
            {
                LuaToken topToken;
                
                while (true)
                {
                    if (operatorStack.Count == 0)
                    {
                        // Unbalanced closing parenthesis is fine if this was a recursive call
                        // Goto is used here in order to break out of the outer loop immediately
                        if (terminateOnDelimiter) goto breakOuter;
                        throw new LuaParsingException("Unbalanced closing parenthesis", token.StartLine,
                            token.StartCol);
                    }

                    topToken = operatorStack.Pop();

                    // Pop operators until we hit the bottom of the operator stack or find the parenthesis that opened
                    // this one
                    if (topToken.Type != OpenExp)
                        output.Push(GetExpressionForOperator(topToken));
                    else break;
                }

                // Grouping an expression inside parentheses does not just change precedence in Lua. Specifically, if
                // the expression is a vararg or function call, it truncates the number of return values to 1. So we
                // need to specifically note if an expression is grouped.
                output.Push(new Expr_Group(topToken.StartLine, topToken.StartCol));
                
                // After closing parenthesis, we now have a prefix expression, and we still expect there not to be a
                // value
                lastWasPrefixExp = true;
                offset++;
                continue;
                
                breakOuter:
                break;
            }

            if (IsOperator(token))
            {
                while (operatorStack.TryPeek(out var topToken) && topToken.Type != OpenExp)
                {
                    var curPrec = Grammar.OperatorPrecedence[token.Type];
                    var topPrec = Grammar.OperatorPrecedence[topToken.Type];

                    if (topPrec > curPrec || (topPrec == curPrec && Grammar.IsLeftAssociative(token.Type)))
                    {
                        output.Push(GetExpressionForOperator(operatorStack.Pop()));
                        continue;
                    }

                    break;
                }
                
                operatorStack.Push(token);

                expectingValue = true;
                lastWasPrefixExp = false;
                offset++;
                continue;
            }
            
            // If the token was not recognized by any of the above, it's a non-expression token, meaning the expression
            // is over. We can break here.
            break;
        }

        if (offset == 0)
        {
            // Returns null despite the expression return being non-nullable; this is intentional. This can only happen
            // if the caller explicitly sets allowZeroLength to true, in which case they should be aware they might get
            // a null return.
            if (allowZeroLength) return (null!, 0);
            
            throw new LuaParsingException(
                "Expression parsed as length 0 (token incorrectly parsed as expression: " +
                $"'{source[position].OriginalOrPlaceholder}')",
                startLine, startCol);
        }

        foreach (var token in operatorStack)
        {
            if (token.Type is OpenExp)
                throw new LuaParsingException("Unbalanced opening parenthesis", token.StartLine, token.StartCol);
            
            output.Push(GetExpressionForOperator(token));
        }
        
        var exp = new Expr_Root(startLine, startCol);
        exp.Initialize(output);

        return (exp, offset);

        // Checks if a token is a value
        bool IsValue(LuaToken token) => token.Type is Numeral or TokenType.String or Name or Nil or True or False 
            or Varargs;

        // Operator token enum values are defined in contiguous chunks in the enum, so this is a succinct way of
        // checking if a token is an operator
        // Technically this also includes IndexName and IndexMethod, but those are already accounted for before we check
        // for operators, so that's fine
        bool IsOperator(LuaToken token)
            => token.Type is >= Add and <= BitwiseNot
                or >= And and <= Not;

        // Parses a call with a single string literal or table constructor argument, starting at the literal. Increments
        // offset to next token automatically.
        void ParseLiteralCall(bool isMethodCall = false)
        {
            var startToken = source[position + offset];

            if (startToken.Type is not (TokenType.String or OpenTable))
                throw new LuaParsingException(
                    "Attempt to parse function call with literal argument starting on token that was not string " +
                    "literal or table constructor", startToken.StartLine, startToken.StartCol);

            if (startToken.Type is TokenType.String)
            {
                output.Push(new Expr_String((string)startToken.Data!, startToken.StartLine, startToken.StartCol));
                offset++;
            }
            else
            {
                var (table, length) = ParseTableConstructor(source, position + offset);
                output.Push(table);
                offset += length;
            }
            
            output.Push(isMethodCall 
                ? new Expr_MethodCall(1, startToken.StartLine, startToken.StartCol) 
                : new Expr_Call(1, startToken.StartLine, startToken.StartCol));
        }
        
        // Parses a call operation, starting at its opening parenthesis
        void ParseCall(bool isMethodCall = false)
        {
            var startToken = source[position + offset];
            
            if (startToken.Type is not OpenExp)
                throw new LuaParsingException("Attempt to parse function call arguments starting on invalid token",
                    startToken.StartLine, startToken.StartCol);
            
            offset++;
            var argc = 0;
            var token = source[position + offset];
            
            while (token.Type is not CloseExp)
            {
                var (expr, length) = ParseExpression(source, position + offset, true);
                output.Push(expr);
                argc++;
                offset += length;

                token = source[position + offset];

                if (token.Type is CloseExp) continue;

                if (token.Type is not Separator)
                    throw new LuaParsingException(token, "expected separator or close to function call arguments");
                
                offset++;
            }
            
            output.Push(isMethodCall 
                ? new Expr_MethodCall(argc, startToken.StartLine, startToken.StartCol) 
                : new Expr_Call(argc, startToken.StartLine, startToken.StartCol));
        }

        Expression GetExpressionForOperator(LuaToken token)
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            => token.Type switch
            {
                And => new Expr_LogicalAnd(token.StartLine, token.StartCol),
                Or => new Expr_LogicalOr(token.StartLine, token.StartCol),
                Not => new Expr_LogicalNot(token.StartLine, token.StartCol),
                Add => new Expr_Add(token.StartLine, token.StartCol),
                Sub => new Expr_Sub(token.StartLine, token.StartCol),
                Mul => new Expr_Mul(token.StartLine, token.StartCol),
                Div => new Expr_Div(token.StartLine, token.StartCol),
                IntDiv => new Expr_IntDiv(token.StartLine, token.StartCol),
                Exp => new Expr_Exp(token.StartLine, token.StartCol),
                Mod => new Expr_Mod(token.StartLine, token.StartCol),
                BitwiseAnd => new Expr_BitwiseAnd(token.StartLine, token.StartCol),
                BitwiseXor => new Expr_BitwiseXor(token.StartLine, token.StartCol),
                BitwiseOr => new Expr_BitwiseOr(token.StartLine, token.StartCol),
                ShiftRight => new Expr_ShiftRight(token.StartLine, token.StartCol),
                ShiftLeft => new Expr_ShiftLeft(token.StartLine, token.StartCol),
                Concat => new Expr_Concat(token.StartLine, token.StartCol),
                LessThan => new Expr_LessThan(false, token.StartLine, token.StartCol),
                LessOrEq => new Expr_LessOrEq(false, token.StartLine, token.StartCol),
                GreaterThan => new Expr_LessThan(true, token.StartLine, token.StartCol),
                GreaterOrEq => new Expr_LessOrEq(true, token.StartLine, token.StartCol),
                EqualTo => new Expr_EqualTo(false, token.StartLine, token.StartCol),
                NotEqualTo => new Expr_EqualTo(true, token.StartLine, token.StartCol),
                Length => new Expr_Length(token.StartLine, token.StartCol),
                Neg => new Expr_Neg(token.StartLine, token.StartCol),
                BitwiseNot => new Expr_BitwiseNot(token.StartLine, token.StartCol),
                _ => throw new LuaParsingException(
                    $"Attempt to get expression for non-operator token '{token.OriginalOrPlaceholder}'",
                    token.StartLine, token.StartCol)
            };
    }

    /// <summary>
    /// Parse a table constructor, starting from its opening curly bracket.
    /// </summary>
    /// <returns>
    /// The table construction expression, and the length of the table constructor. Length includes the final closing
    /// curly bracket of the table constructor.
    /// </returns>
    public static (Expr_Table Table, int Length) ParseTableConstructor(LuaToken[] source, int position)
    {
        var startToken = source[position];

        if (startToken.Type is not OpenTable)
            throw new LuaParsingException($"Attempt to parse table constructor starting on invalid token",
                startToken.StartLine, startToken.StartCol);

        var keyed = new List<(Expression Key, Expression Value)>();
        var unkeyed = new List<Expression>();
        
        var offset = 1;
        while (source[position + offset] is LuaToken { Type: not CloseTable } token)
        {
            // Keyed entry with Name as key
            if (token.Type is Name && source[position + offset + 1].Type is Assign)
            {
                var key = new Expr_String((string)token.Data!, token.StartLine, token.StartCol);
                var (value, length) = ParseExpression(source, position + offset + 2, true);
                
                keyed.Add((key, value));
                
                offset += length + 2;
            }
            // Keyed entry with expression as key
            else if (token.Type is OpenIndex)
            {
                var (key, keyLength) = ParseExpression(source, position + offset + 1, true);
                offset += keyLength + 1;

                var closeIndexToken = source[position + offset];
                if (closeIndexToken.Type is not CloseIndex)
                    throw new LuaParsingException(closeIndexToken, "expected close to index opened on line " +
                                                                   $"{token.StartLine}, column {token.StartCol}");

                var assignToken = source[position + offset + 1];
                if (assignToken.Type is not Assign)
                    throw new LuaParsingException(assignToken, "expected assignment between expression index " + 
                                                               "and expression value in table constructor");

                var (value, valueLength) = ParseExpression(source, position + offset + 2, true);
                
                keyed.Add((key, value));
                
                offset += valueLength + 2;
            }
            // Otherwise, unkeyed entry
            else
            {
                var (value, length) = ParseExpression(source, position + offset, true);
                
                unkeyed.Add(value);
                
                offset += length;
            }
            
            // After parsing entry, check for comma or semicolon, skip if there. If close table, do not skip. If
            // anything else, throw error.
            var nextToken = source[position + offset];
            if (nextToken.Type is CloseTable) continue;
            if (nextToken.Type is Separator or TokenType.Statement) offset++;
            else
                throw new LuaParsingException(nextToken, "expected delimiter between table entries or close " +
                                                         "to table constructor");
        }

        return (new Expr_Table(keyed, unkeyed, startToken.StartLine, startToken.StartCol), offset + 1);
    }
}