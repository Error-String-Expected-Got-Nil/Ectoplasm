using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Runtime.Values;

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
            if (token.Type is TokenType.CloseIndex or TokenType.CloseTable)
            {
                if (terminateOnDelimiter) break;
                throw new LuaParsingException($"Unexpected token '{token.OriginalString}'", token.StartLine,
                    token.StartCol);
            }
            
            if (IsValue(token))
            {
                if (!expectingValue)
                {
                    if (lastWasPrefixExp && token.Type is TokenType.String or TokenType.OpenTable)
                    {
                        // TODO: Not an error, this is a function call with the string or table literal.
                        //  Set lastWasPrefixExp to true and expectingValue to false
                        throw new NotImplementedException();
                    }

                    // If we see a value when we aren't expecting to, and it isn't a literal argument to a prefix
                    // expression, then that is invalid.
                    throw new LuaParsingException($"Unexpected value '{(token.Type == TokenType.String ? "<string>"
                        : token.OriginalString)}'", token.StartLine, token.StartCol);
                }
                
                // Otherwise, we push it to the output stack immediately.
                output.Push(token.Type switch
                {
                    TokenType.Name => new Expr_Variable((string)token.Data!, token.StartLine, token.StartCol),
                    TokenType.String => new Expr_String((string)token.Data!, token.StartLine, token.StartCol),
                    TokenType.Varargs => new Expr_Varargs(token.StartLine, token.StartCol),
                    _ => new Expr_Value(new LuaValue(token), token.StartLine, token.StartCol)
                });

                expectingValue = false;
                lastWasPrefixExp = token.Type == TokenType.Name;

                offset++;
                continue;
            }

            if (token.Type is TokenType.Function)
            {
                // If we find a function declaration, and we aren't expecting a value, that's the next statement, we can
                // end parsing
                if (!expectingValue) break;
                
                // Otherwise, parse the function as a value
                // TODO: Parse function def
                throw new NotImplementedException();
            }

            if (token.Type is TokenType.OpenExp)
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
                
                output.Push(ParseCall());
                offset++; // Offset will be on closing parenthesis after ParseCall(), need to increment
                continue;
            }
            
            // TODO: Parse table constructors here
            
            // At this point, we know we're parsing an operator of some kind. If we were expecting a value, this is only
            // valid if it's a unary operator.
            if (expectingValue)
            {
                if (!Grammar.UnaryOperators.Contains(token.Type))
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (expected value or unary operator token)", 
                        token.StartLine, token.StartCol);

                // For tokens that could be either unary or binary, replace the binary token with its unary counterpart
                if (token.Type is TokenType.Sub)
                    token = token with { Type = TokenType.Neg };
                else if (token.Type is TokenType.BitwiseXor)
                    token = token with { Type = TokenType.BitwiseNot };
            }
            
            if (token.Type is TokenType.IndexName)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (index operator must be preceded by a Name, " +
                        "index operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

                var next = source[position + offset + 1];
                
                if (next.Type is not TokenType.Name)
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

            if (token.Type is TokenType.IndexMethod)
            {
                if (!lastWasPrefixExp)
                    throw new LuaParsingException(
                        $"Unexpected token '{token.OriginalString}' (method operator must be preceded by a Name, " +
                        "index operation, function call, or parenthesized expression)", 
                        token.StartLine, token.StartCol);

                var next = source[position + offset + 1];
                
                if (next.Type is not TokenType.Name)
                    throw new LuaParsingException(
                        $"Unexpected token '{next.OriginalString}' (expected Name operand for method operator)",
                        next.StartLine, next.StartCol);

                // Source will always end with an EndOfChunk token, so we know there will always be a second-next token
                // if the next token wasn't an EndOfChunk (and we currently know it's a Name)
                var argsStart = source[position + offset + 2];

                if (argsStart.Type is not TokenType.OpenExp)
                    throw new LuaParsingException(
                        $"Unexpected token '{argsStart.OriginalString}' (expected arguments for method operator)",
                        argsStart.StartLine, argsStart.StartCol);
                
                output.Push(new Expr_String((string)next.Data!, next.StartLine, next.StartCol));
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                offset += 2;
                output.Push(ParseCall(true));
                offset++;
                continue;
            }

            if (token.Type is TokenType.OpenIndex)
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
                if (close.Type is not TokenType.CloseIndex)
                    throw new LuaParsingException(
                        $"Unexpected token '{close.OriginalString}' (expected close to index operation on line " +
                        $"{token.StartLine}, column {token.StartCol})", close.StartLine, close.StartCol);
                
                output.Push(operand);
                output.Push(new Expr_Index(token.StartLine, token.StartCol));
                offset++;
                continue;
            }

            if (token.Type is TokenType.CloseExp)
            {
                while (true)
                {
                    if (operatorStack.Count == 0)
                    {
                        // Unbalanced closing parenthesis is fine if this was a recursive call
                        if (terminateOnDelimiter) break;
                        throw new LuaParsingException("Unbalanced closing parenthesis", token.StartLine,
                            token.StartCol);
                    }

                    var topToken = operatorStack.Pop();

                    // Pop operators until we hit the bottom of the operator stack or find the parenthesis that opened
                    // this one
                    if (topToken.Type != TokenType.CloseExp)
                        output.Push(GetExpressionForOperator(topToken));
                    else break;
                }

                // After closing parenthesis, we now have a prefix expression, and we still expect there not to be a
                // value
                lastWasPrefixExp = true;
                offset++;
                continue;
            }

            if (IsOperator(token))
            {
                while (operatorStack.TryPeek(out var topToken) && topToken.Type != TokenType.OpenExp)
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
                $"Expression parsed as length 0 (token incorrectly parsed as expression: " +
                $"'{(source[position].Type is TokenType.String ? "<string>" : source[position].OriginalString)}')", 
                startLine, startCol);
        
        // TODO: Resolve and initialize expression from output stack

        return (null!, 0);

        // Checks if a token is a value
        bool IsValue(LuaToken token) => token.Type is TokenType.Numeral or TokenType.String or TokenType.Name
            or TokenType.Nil or TokenType.True or TokenType.False or TokenType.Varargs;

        // Operator token enum values are defined in contiguous chunks in the enum, so this is a succinct way of
        // checking if a token is an operator
        // Technically this also includes IndexName and IndexMethod, but those are already accounted for before we check
        // for operators, so that's fine
        bool IsOperator(LuaToken token)
            => token.Type is >= TokenType.Add and <= TokenType.BitwiseNot
                or >= TokenType.And and <= TokenType.Not;

        // Parses a call operation, starting at its opening parenthesis
        Expr_Call ParseCall(bool isMethodCall = false)
        {
            // TODO: Parse function call operation
            //  Make sure to increment offset by length of call operation
            throw new NotImplementedException();
        }

        Expression GetExpressionForOperator(LuaToken token)
        {
            // TODO: Get expression for operator token
            throw new NotImplementedException();
        }
    }
}