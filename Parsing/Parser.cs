using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Parsing.Expressions.BinaryOperators;
using Ectoplasm.Parsing.Expressions.UnaryOperators;
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
    /// [FILL IN DOCUMENTATION]
    /// </summary>
    /// <param name="source">Source token sequence to parse.</param>
    /// <param name="position">Index in source to start parsing at.</param>
    /// <param name="terminateOnDelimiter">
    /// On encountering an unexpected delimiter token, the parser will simply stop parsing instead of throwing an error.
    /// Only applies to closing parentheses, closing brackets, and closing curly brackets. This is used for recursive
    /// calls.
    /// </param>
    /// <returns>The parsed expression, and the number of tokens that make it up.</returns>
    // This is a variant of Dijkstra's shunting yard algorithm adapted for Lua's syntax, and which is able to validate
    // expressions as it parses them.
    public static (Expr_Root Expr, int Length) ParseExpression(LuaToken[] source, int position, 
        bool terminateOnDelimiter = false)
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
                throw new LuaParsingException($"Unexpected token '{token.OriginalString}'", token.StartLine,
                    token.StartCol);
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
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (function call must be preceded by a Name, index " +
                        "operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

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
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (expected value or unary operator token)", 
                        token.StartLine, token.StartCol);

                // For tokens that could be either unary or binary, replace the binary token with its unary counterpart
                if (token.Type is Sub)
                    token = token with { Type = Neg };
                else if (token.Type is BitwiseXor)
                    token = token with { Type = BitwiseNot };
            }
            
            if (token.Type is IndexName)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (index operator must be preceded by a Name, " +
                        "index operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

                var next = source[position + offset + 1];
                
                if (next.Type is not Name)
                    throw new LuaParsingException(
                        $"Unexpected token '{next.OriginalString}' (expected Name operand for index operator)",
                        next.StartLine, next.StartCol);
                
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
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (method operator must be preceded by a Name, " +
                        "index operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

                var next = source[position + offset + 1];
                
                if (next.Type is not Name)
                    throw new LuaParsingException(
                        $"Unexpected token '{next.OriginalString}' (expected Name operand for method operator)",
                        next.StartLine, next.StartCol);

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
                    throw new LuaParsingException(
                        $"Unexpected token '{argsStart.OriginalString}' (expected arguments for method operator)",
                        argsStart.StartLine, argsStart.StartCol);
                
                ParseCall(true); // Parse call pushes the call expression itself
                offset++;
                continue;
            }

            if (token.Type is OpenIndex)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (index operator must be preceded by a Name, " +
                        "index operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

                // Index operation in brackets contains an expression, can recursively parse
                var (operand, length) = ParseExpression(source, position + offset + 1, 
                    true);
                
                offset += length + 1;
                var close = source[position + offset];
                if (close.Type is not CloseIndex)
                    throw new LuaParsingException(
                        $"Unexpected token '{close.OriginalString}' (expected close to index operation on line " +
                        $"{token.StartLine}, column {token.StartCol})", close.StartLine, close.StartCol);
                
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
            throw new LuaParsingException(
                "Expression parsed as length 0 (token incorrectly parsed as expression: " +
                $"'{source[position].OriginalOrPlaceholder}')", 
                startLine, startCol);

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
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalOrPlaceholder}' (expected separator or close to function " +
                        "call arguments)", token.StartLine, token.StartCol);
                
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
                offset += keyLength;

                var closeIndexToken = source[position + offset];
                if (closeIndexToken.Type is not CloseIndex)
                    throw new LuaParsingException(
                        $"Unexpected token '{closeIndexToken.OriginalOrPlaceholder}' (expected close to index opened " +
                        $"on line {token.StartLine}, column {token.StartCol})", closeIndexToken.StartLine,
                        closeIndexToken.StartCol);

                var assignToken = source[position + offset + 1];
                if (assignToken.Type is not Assign)
                    throw new LuaParsingException(
                        $"Unexpected token '{assignToken.OriginalOrPlaceholder}' (expected assignment between " +
                        "expression index and expression value in table constructor)");

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
            if (nextToken.Type is Separator or Statement) offset++;
            else
                throw new LuaParsingException(
                    $"Unexpected token '{nextToken.OriginalOrPlaceholder}' (expected delimiter between table entries " +
                    "or close to table constructor)", nextToken.StartLine, nextToken.StartCol);
        }

        return (new Expr_Table(keyed, unkeyed, startToken.StartLine, startToken.StartCol), offset + 1);
    }
}