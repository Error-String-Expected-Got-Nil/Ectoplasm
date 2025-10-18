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
    /// <param name="source">Enumerator of <see cref="LuaToken"/>s. Should not contain any tokens of type
    /// <see cref="TokenType.Whitespace"/> or <see cref="TokenType.Comment"/>, discard them before providing a sequence
    /// to the parser. The sequence should also end with an <see cref="TokenType.EndOfChunk"/> token.
    /// </param>
    /// <returns>[FILL IN DOCUMENTATION]</returns>
    /// <exception cref="LuaParsingException">
    /// [FILL IN DOCUMENTATION]
    /// </exception>
    public static ParsedChunk Parse(IEnumerator<LuaToken> source)
    {
        return null!;
    }

    /// <summary>
    /// Parse a sequence of <see cref="LuaToken"/>s as a block of statements.
    /// </summary>
    /// <returns>The list of statements that make up the block.</returns>
    public static List<Statement> ParseBlock(IEnumerator<LuaToken> source, string? sourceName = null)
    {
        var statements = new List<Statement>();
        
        while (true)
        {
            var token = source.Current;

            switch (token.Type)
            {
                case TokenType.Statement: ParseStatement(); continue;
                case LabelSep: ParseLabelSep(); continue;
                case Goto: ParseGoto(); continue;
                case Break: ParseBreak(); continue;
                case Do: ParseDo(); continue;
                case While: ParseWhile(); continue;
                case Repeat: ParseRepeat(); continue;
                case If: ParseIf(); continue;
                case For: ParseFor(); continue;
                case Local: ParseLocal(); continue;
                
                // TODO: Function definitions, function call statements, assignment statements
            }

            continue;
            
            #region Parsing Functions
            
            void ParseLocal()
            {
                source.MoveNext();

                if (source.Current.Type is Function)
                {
                    // TODO: Parse local function def
                    throw new NotImplementedException();
                }
                
                // Not a local function def, must be followed by an attnamelist
                var attnamelist = ParseAttNamelist(source, sourceName);
                List<Expression>? expressions = null;

                // Declaration also includes assignment
                if (source.Current.Type is Assign)
                {
                    source.MoveNext();
                    expressions = ParseExplist(source, sourceName);
                }
                
                statements.Add(new Stat_LocalDeclaration(attnamelist, expressions, token.StartLine, token.StartCol));
            }
            
            void ParseFor()
            {
                source.MoveNext();
                var namelist = ParseNamelist(source, sourceName);

                var followingToken = source.Current;
                    
                if (followingToken.Type is Assign)
                {
                    // For statement starting with initial assignment must be followed by a separator, then an
                    // expression, then optionally another separator and expression.

                    if (namelist.Count != 1)
                        throw new LuaParsingException("For loop with initial assignment may only define one name",
                            token.StartLine, token.StartCol, sourceName);

                    source.MoveNext();
                    var initExp = ParseExpression(source, sourceName);

                    // Separator between assignment and end expression
                    var initEndSep = source.Current;
                    if (initEndSep.Type is not Separator)
                        throw new LuaParsingException(initEndSep, 
                            "expected separator between assignment and end expressions of for statement " +
                            $"started on line {token.StartLine}, column {token.StartCol}", sourceName);

                    source.MoveNext();
                    var endExp = ParseExpression(source, sourceName);

                    // Token following the end expression. If it is a separator, we parse the increment expression
                    // after it, then set it to the token following that expression. Otherwise, we continue to
                    // verifying it's a 'do' token.
                    var forContinuationToken = source.Current;

                    Expr_Root? incExp = null;
                    if (forContinuationToken.Type is Separator)
                    {
                        source.MoveNext();
                        incExp = ParseExpression(source, sourceName);
                        forContinuationToken = source.Current;
                    }

                    if (forContinuationToken.Type is not Do)
                        throw new LuaParsingException(forContinuationToken,
                            "expected 'do' to delimit header of for statement started on line " +
                            $"{token.StartLine}, column {token.StartCol}", sourceName);

                    source.MoveNext();
                    var forBlockContents = ParseBlock(source);

                    var forEndToken = source.Current;
                    if (forEndToken.Type is not End)
                        throw new LuaParsingException(forEndToken,
                            $"expected end to for loop started on line {token.StartLine}, " +
                            $"column {token.StartCol}", sourceName);
                        
                    statements.Add(new Stat_ForNumeric(namelist[0], initExp, endExp, incExp, 
                        forBlockContents, token.StartLine, token.StartCol));
                    source.MoveNext();
                    return;
                }
                    
                if (followingToken.Type is In)
                {
                    source.MoveNext();

                    var explist = ParseExplist(source, sourceName);

                    var forDoToken = source.Current;
                    if (forDoToken.Type is not Do)
                        throw new LuaParsingException(forDoToken,
                            "expected 'do' to delimit header of for statement started on line " +
                            $"{token.StartLine}, column {token.StartCol}", sourceName);

                    source.MoveNext();
                    var forBlockContents = ParseBlock(source);
                        
                    var forEndToken = source.Current;
                    if (forEndToken.Type is not End)
                        throw new LuaParsingException(forEndToken,
                            $"expected end to for loop started on line {token.StartLine}, " +
                            $"column {token.StartCol}", sourceName);
                        
                    statements.Add(new Stat_ForGeneric(namelist, explist, forBlockContents, token.StartLine, 
                        token.StartCol));
                    source.MoveNext();
                    return;
                }
                    
                // Following token was not an Assign or In token
                throw new LuaParsingException(followingToken, 
                    "expected 'in' or '=' after name or namelist at beginning of for statement started on " + 
                    $"line {token.StartLine}, column {token.StartCol}", sourceName);
            }

            void ParseIf()
            {
                source.MoveNext();
                var ifExp = ParseExpression(source, sourceName);
                    
                var ifThenToken = source.Current;
                if (ifThenToken.Type is not Then)
                    throw new LuaParsingException(ifThenToken,
                        "expected 'then' to delimit expression of if statement started on " +
                        $"line {token.StartLine}, column {token.StartCol}", sourceName);

                source.MoveNext();
                var ifBlockContents = ParseBlock(source, sourceName);

                var ifClauses = new List<(Expression Condition, List<Statement> Block)> 
                    { (ifExp, ifBlockContents) };
                List<Statement>? elseBlock = null;
                    
                while (true)
                {
                    var ifContinuationToken = source.Current;
                        
                    if (ifContinuationToken.Type is Else)
                    {
                        source.MoveNext();
                        elseBlock = ParseBlock(source);
                        break;
                    }

                    if (ifContinuationToken.Type is Elseif)
                    {
                        source.MoveNext();
                        var elseifExp = ParseExpression(source, sourceName);

                        var elseifThenToken = source.Current;
                        if (elseifThenToken.Type is not Then)
                            throw new LuaParsingException(elseifThenToken,
                                "expected 'then' to delimit expression of elseif clause started on " +
                                $"line {ifContinuationToken.StartLine}, column {ifContinuationToken.StartCol}", 
                                sourceName);

                        source.MoveNext();
                        var elseifBlockContents = ParseBlock(source);
                            
                        ifClauses.Add((elseifExp, elseifBlockContents));
                        continue;
                    }

                    break;
                }

                var ifEndToken = source.Current;
                if (ifEndToken.Type is not End)
                    throw new LuaParsingException(ifEndToken, 
                        $"expected end to if statement started on line {token.StartLine}, column {token.StartCol}", 
                        sourceName);
                    
                statements.Add(new Stat_If(ifClauses, elseBlock, token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseRepeat()
            {
                source.MoveNext();
                var repeatBlockContents = ParseBlock(source);
                    
                var repeatUntilToken = source.Current;
                if (repeatUntilToken.Type is not Until)
                    throw new LuaParsingException(repeatUntilToken,
                        "expected 'until' to delimit block of repeat statement started on " +
                        $"line {token.StartLine}, column {token.StartCol}", sourceName);

                source.MoveNext();
                var untilExp = ParseExpression(source, sourceName);

                statements.Add(new Stat_Repeat(untilExp, repeatBlockContents, token.StartLine, token.StartCol));
            }

            void ParseWhile()
            {
                source.MoveNext();
                var whileExp = ParseExpression(source, sourceName);

                var whileDoToken = source.Current;
                if (whileDoToken.Type is not Do)
                    throw new LuaParsingException(whileDoToken,
                        "expected 'do' to delimit expression of while loop started on line " +
                        $"{token.StartLine}, column {token.StartCol}", sourceName);

                source.MoveNext();
                var whileBlockContents = ParseBlock(source);
                    
                var whileEndToken = source.Current;
                if (whileEndToken.Type is not End)
                    throw new LuaParsingException(whileEndToken,
                        "expected end to while statement do block started on " +
                        $"line {whileDoToken.StartLine}, column {whileDoToken.StartCol}", sourceName);

                statements.Add(new Stat_While(whileExp, whileBlockContents, token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseDo()
            {
                source.MoveNext();
                var doBlockContents = ParseBlock(source);
                    
                var doBlockEndToken = source.Current;
                if (doBlockEndToken.Type is not End)
                    throw new LuaParsingException(doBlockEndToken, 
                        $"expected end to do block started on line {token.StartLine}, column {token.StartCol}", 
                        sourceName);
                    
                statements.Add(new Stat_Do(doBlockContents, token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseBreak()
            {
                statements.Add(new Stat_Break(token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseGoto()
            {
                source.MoveNext();
                var targetLabelName = source.Current;

                if (targetLabelName.Type is not Name)
                    throw new LuaParsingException(targetLabelName, "expected Name of target label", sourceName);
                    
                statements.Add(new Stat_Goto((string)targetLabelName.Data!, token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseLabelSep()
            {
                source.MoveNext();
                var labelName = source.Current;
                    
                if (labelName.Type is not Name)
                    throw new LuaParsingException(labelName, "expected Name of label", sourceName);

                source.MoveNext();
                var labelEndToken = source.Current;
                if (labelEndToken.Type is not LabelSep)
                    throw new LuaParsingException(labelEndToken, "expected closing label separator", sourceName);
                    
                statements.Add(new Stat_Label((string)labelName.Data!, token.StartLine, token.StartCol));
                source.MoveNext();
            }

            void ParseStatement()
            {
                statements.Add(new Stat_Empty(token.StartLine, token.StartCol));
                source.MoveNext();
            }
            
            #endregion
        }

        // TODO: Ensure enumerator ends on token following last token in block
        return statements;
    }

    private static List<(LuaToken name, LocalAttribute Attribute)> ParseAttNamelist(IEnumerator<LuaToken> source, 
        string? sourceName)
    {
        var names = new List<(LuaToken, LocalAttribute)>();

        while (true)
        {
            var token = source.Current;
            
            if (token.Type is not Name)
                throw new LuaParsingException(token,
                    $"expected Name in attnamelist, got {token.Type} instead", sourceName);

            var name = token;
            var attribute = LocalAttribute.None;

            source.MoveNext();
            if (source.Current.Type is LessThan /* < */)
            {
                // This local name appears to have an attribute attached, let's check that the syntax is correct and
                // that it's a valid attribute.

                source.MoveNext();
                if (source.Current.Type is not Name)
                    throw new LuaParsingException(source.Current,
                        $"expected Name for attribute of local variable defined on line {token.StartLine}, " +
                        $"column {token.StartCol}, got {source.Current.Type} instead", sourceName);

                var attributeName = (string)source.Current.Data!;
                attribute = attributeName switch
                {
                    "const" => LocalAttribute.Const,
                    "close" => LocalAttribute.Close,
                    _ => throw new LuaParsingException(source.Current, 
                        $"attribute name for local variable defined on line {token.StartLine}, column " +
                        $"{token.StartCol} was not valid; valid attribute names are 'const' and 'close', got " +
                        $"'{attributeName}' instead", sourceName)
                };

                source.MoveNext();
                if (source.Current.Type is not GreaterThan /* > */)
                    throw new LuaParsingException(source.Current,
                        "expected '>' to close attribute tag of local variable defined on line " +
                        $"{token.StartLine}, column {token.StartCol}", sourceName);

                // Move to next token, should be separator or end of attnamelist
                source.MoveNext();
            }
            
            names.Add((name, attribute));

            if (source.Current.Type is not Separator)
                return names;

            source.MoveNext();
        }
    }
    
    private static List<LuaToken> ParseNamelist(IEnumerator<LuaToken> source, string? sourceName)
    {
        var names = new List<LuaToken>();
        
        while (true)
        {
            var token = source.Current;

            if (token.Type is not Name)
                throw new LuaParsingException(token, $"expected Name in namelist, got {token.Type} instead", 
                    sourceName);
            
            names.Add(token);

            source.MoveNext();
            if (source.Current.Type is not Separator)
                return names;
            
            source.MoveNext();
        }
    }

    // Very simple function which parses a list of expressions delimited by Separator tokens.
    private static List<Expression> ParseExplist(IEnumerator<LuaToken> source, string? sourceName)
    {
        var exps = new List<Expression>();

        while (true)
        {
            exps.Add(ParseExpression(source, sourceName));

            if (source.Current.Type is not Separator)
                return exps;
        }
    }
    
    /// <summary>
    /// Parses a Lua expression starting at a given position in a sequence of source tokens. 
    /// </summary>
    /// <param name="source">
    /// Enumerator over the source token sequence to parse. Enumerator will be on the token following the expression
    /// after parsing.
    /// </param>
    /// <param name="sourceName">
    /// Name given to the source text where this expression is being parsed from. Used for more helpful error reporting.
    /// Source name will be omitted in exceptions if null, but line and column numbers will still be kept.
    /// </param>
    /// <param name="terminateOnDelimiter">
    /// On encountering an unexpected delimiter token, the parser will simply stop parsing instead of throwing an error.
    /// Only applies to closing parentheses, closing brackets, and closing curly brackets. This is used for recursive
    /// calls.
    /// </param>
    /// <param name="allowZeroLength">
    /// If true, an exception will not be thrown if the expression is parsed as zero-length. In this case, the returned
    /// expression will be null, and the source enumerator will not have moved from its initial position.
    /// </param>
    /// <returns>The parsed expression.</returns>
    // This is a variant of Dijkstra's shunting yard algorithm adapted for Lua's syntax, and which is able to validate
    // expressions as it parses them.
    public static Expr_Root ParseExpression(IEnumerator<LuaToken> source, string? sourceName = null,
        bool terminateOnDelimiter = false, bool allowZeroLength = false)
    {
        var startLine = source.Current.StartLine;
        var startCol = source.Current.StartCol;
        
        // Flag indicating if we expect the next token to be a value, rather than an operator or some other symbol.
        // Used to both validate expressions and distinguish unary operators.
        var expectingValue = true;

        // Flag indicating if the last parsed portion of the expression was a prefix expression per Lua's syntax.
        // That is: A Name, an index operation, a function call, or a parenthesized expression.
        var lastWasPrefixExp = false;

        var output = new Stack<Expression>();
        var operatorStack = new Stack<LuaToken>();
        
        // Loop is terminated by encountering token that cannot be part of an expression. This will always happen due to
        // the EndOfChunk token.
        while (true)
        {
            var token = source.Current;

            // Closing index or table is always invalid mid-expression, unless this was a recursive call, in which case
            // we might be parsing an expression inside a table or index, so we just break.
            if (token.Type is CloseIndex or CloseTable)
            {
                if (terminateOnDelimiter) break;
                throw new LuaParsingException(token, null, sourceName);
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

                source.MoveNext();
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
                    source.MoveNext();
                    continue;
                }

                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, 
                        "function call must be preceded by a Name, index operation, function call, or " +
                        "parenthesized expression", sourceName);

                ParseCall(); // ParseCall pushes the call expression itself
                source.MoveNext(); // Offset will be on closing parenthesis after ParseCall(), need to increment
                continue;
            }
            
            if (token.Type is OpenTable)
            {
                var table = ParseTableConstructor(source, sourceName);
                output.Push(table);
                expectingValue = false;
                lastWasPrefixExp = false;
                continue;
            }
            
            // At this point, we know we're parsing an operator of some kind. If we were expecting a value, this is only
            // valid if it's a unary operator.
            if (expectingValue)
            {
                if (!Grammar.UnaryOperators.Contains(token.Type))
                    throw new LuaParsingException(token, "expected value or unary operator token", sourceName);

                // For tokens that could be either unary or binary, replace the binary token with its unary counterpart
                if (token.Type is Sub)
                    token = token with { Type = Neg };
                else if (token.Type is BitwiseXor)
                    token = token with { Type = BitwiseNot };
            }
            
            if (token.Type is IndexName)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, 
                        "index operator must be preceded by a Name, index operation, function call, or " +
                        "parenthesized expression", sourceName);

                source.MoveNext();
                var next = source.Current;
                
                if (next.Type is not Name)
                    throw new LuaParsingException(next, "expected Name operand for index operator", sourceName);
                
                // Index operations are evaluated without precedence, we simply push them immediately to the output
                // For the dot indexing form, the "Name" operand is actually syntactic sugar for a string literal
                output.Push(new Expr_String((string)next.Data!, next.StartLine, next.StartCol));
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                source.MoveNext();
                continue;
            }

            if (token.Type is IndexMethod)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, 
                        "method operator must be preceded by a Name, index operation, function call, or " +
                        "parenthesized expression", sourceName);

                source.MoveNext();
                var next = source.Current;
                
                if (next.Type is not Name)
                    throw new LuaParsingException(next, "expected Name operand for method operator", sourceName);

                output.Push(new Expr_String((string)next.Data!, next.StartLine, next.StartCol));
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                source.MoveNext();
                
                // Source will always end with an EndOfChunk token, so we know there will always be a second-next token
                // if the next token wasn't an EndOfChunk (and we currently know it's a Name)
                var argsStart = source.Current;
                
                if (argsStart.Type is TokenType.String or OpenTable)
                {
                    ParseLiteralCall(true);
                    continue;
                }
                
                if (argsStart.Type is not OpenExp)
                    throw new LuaParsingException(argsStart, "expected arguments for method operator", sourceName);
                
                ParseCall(true); // Parse call pushes the call expression itself
                source.MoveNext();
                continue;
            }

            if (token.Type is OpenIndex)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(token, 
                        "index operator must be preceded by a Name, index operation, function call, or " +
                        "parenthesized expression", sourceName);

                // Index operation in brackets contains an expression, can recursively parse
                var operand = ParseExpression(source, sourceName, true);

                source.MoveNext();
                var close = source.Current;
                if (close.Type is not CloseIndex)
                    throw new LuaParsingException(close, 
                        $"expected close to index operation on line {token.StartLine}, column {token.StartCol}", 
                        sourceName);
                
                output.Push(operand);
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                source.MoveNext();
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
                            token.StartCol, sourceName);
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
                source.MoveNext();
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
                source.MoveNext();
                continue;
            }
            
            // If the token was not recognized by any of the above, it's a non-expression token, meaning the expression
            // is over. We can break here.
            break;
        }

        // If the final output stack is empty, then there were no values parsed from the expression, meaning it was
        // either an expression containing only operator tokens (which is invalid and would have already resulted in an
        // exception), or it was an empty expression.
        if (output.Count == 0)
        {
            // Returns null despite the expression return being non-nullable; this is intentional. This can only happen
            // if the caller explicitly sets allowZeroLength to true, in which case they should be aware they might get
            // a null return.
            if (allowZeroLength) return null!;
            
            throw new LuaParsingException(
                "Expression parsed as length 0 (token incorrectly parsed as expression: " +
                $"'{source.Current.OriginalOrPlaceholder}')",
                startLine, startCol, sourceName);
        }

        foreach (var token in operatorStack)
        {
            if (token.Type is OpenExp)
                throw new LuaParsingException("Unbalanced opening parenthesis", token.StartLine, token.StartCol, 
                    sourceName);
            
            output.Push(GetExpressionForOperator(token));
        }
        
        var exp = new Expr_Root(startLine, startCol);
        exp.Initialize(output);

        return exp;

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
            var startToken = source.Current;

            if (startToken.Type is not (TokenType.String or OpenTable))
                throw new LuaParsingException(
                    "Attempt to parse function call with literal argument starting on token that was not string " +
                    "literal or table constructor", startToken.StartLine, startToken.StartCol, sourceName);

            if (startToken.Type is TokenType.String)
            {
                output.Push(new Expr_String((string)startToken.Data!, startToken.StartLine, startToken.StartCol));
                source.MoveNext();
            }
            else
            {
                output.Push(ParseTableConstructor(source, sourceName));
            }

            output.Push(isMethodCall 
                ? new Expr_MethodCall(1, startToken.StartLine, startToken.StartCol) 
                : new Expr_Call(1, startToken.StartLine, startToken.StartCol));
        }
        
        // Parses a call operation, starting at its opening parenthesis
        void ParseCall(bool isMethodCall = false)
        {
            var startToken = source.Current;
            
            if (startToken.Type is not OpenExp)
                throw new LuaParsingException("Attempt to parse function call arguments starting on invalid token",
                    startToken.StartLine, startToken.StartCol, sourceName);
            
            source.MoveNext();
            var argc = 0;
            var token = source.Current;
            
            while (token.Type is not CloseExp)
            {
                var expr = ParseExpression(source, sourceName, true);
                output.Push(expr);
                argc++;
                source.MoveNext();

                token = source.Current;

                if (token.Type is CloseExp) continue;

                if (token.Type is not Separator)
                    throw new LuaParsingException(token, "expected separator or close to function call arguments", 
                        sourceName);
                
                source.MoveNext();
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
                    token.StartLine, token.StartCol, sourceName)
            };
    }

    /// <summary>
    /// Parse a table constructor, starting from its opening curly bracket. Source enumerator will be positioned on the
    /// token following the closing curly bracket after parsing. 
    /// </summary>
    /// <returns> The table construction expression. </returns>
    public static Expr_Table ParseTableConstructor(IEnumerator<LuaToken> source, string? sourceName)
    {
        var startToken = source.Current;

        if (startToken.Type is not OpenTable)
            throw new LuaParsingException("Attempt to parse table constructor starting on invalid token",
                startToken.StartLine, startToken.StartCol, sourceName);

        var keyed = new List<(Expression Key, Expression Value)>();
        var unkeyed = new List<Expression>();

        source.MoveNext();
        while (true)
        {
            var token = source.Current;
            
            // Keyed entry
            if (token.Type is OpenIndex)
            {
                source.MoveNext();
                var key = ParseExpression(source, sourceName, true);
                
                // Enumerator will be on token following expression after parsing an expression.
                var closeIndexToken = source.Current;
                if (closeIndexToken.Type is not CloseIndex)
                    throw new LuaParsingException(closeIndexToken, 
                        $"expected close to index opened on line {token.StartLine}, column {token.StartCol}", 
                        sourceName);

                source.MoveNext();
                var assignToken = source.Current;
                if (assignToken.Type is not Assign)
                    throw new LuaParsingException(assignToken, 
                        "expected assignment between expression index and expression value in table constructor", 
                        sourceName);

                source.MoveNext();
                var value = ParseExpression(source, sourceName, true);
                
                keyed.Add((key, value));
            }
            // Otherwise, unkeyed entry or syntactic sugar keyed entry
            else
            {
                var unkeyedExpr = ParseExpression(source, sourceName, true);

                // After parsing the expression, check if the next token is an Assign token. If so, this might have been
                // the syntactic sugar form for a keyed entry.
                if (source.Current.Type is Assign)
                {
                    // Said form requires the prior token be a lone Name. The parsed expression will only consist of an
                    // Expr_Variable in that case, so we can check that to see if it was. If not, then this is a syntax
                    // error.
                    if (unkeyedExpr.Name is not { } keyName)
                        throw new LuaParsingException(source.Current, 
                            "expected separator between entries in table constructor", sourceName);
                    
                    source.MoveNext();
                    var value = ParseExpression(source, sourceName, true);
                    
                    keyed.Add((new Expr_String(keyName, unkeyedExpr.StartLine, unkeyedExpr.StartCol), value));
                }
                else
                {
                    unkeyed.Add(unkeyedExpr);
                }
            }
            
            // After parsing entry, check for comma or semicolon, skip if there. If close table, exit loop. If anything
            // else, throw error.
            var nextToken = source.Current;
            if (nextToken.Type is CloseTable) break;
            if (nextToken.Type is Separator or TokenType.Statement) source.MoveNext();
            else
                throw new LuaParsingException(nextToken,
                    "expected delimiter between table entries or close to table constructor", sourceName);
        }

        // Loop exits on encountering CloseTable token. Advance to token following it.
        source.MoveNext();
        return new Expr_Table(keyed, unkeyed, startToken.StartLine, startToken.StartCol);
    }
}