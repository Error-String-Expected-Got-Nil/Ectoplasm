using Ectoplasm.Lexing;
using Ectoplasm.Runtime.Values;
using Ectoplasm.SimpleExpressions.Operators;
using LuaValue = Ectoplasm.Runtime.Values.LuaValue;

namespace Ectoplasm.SimpleExpressions;

public static class SimpleExpressionParser
{
    /// <summary>
    /// Parses a simple standalone Lua expression using Dijkstra's shunting yard algorithm. Only accepts numerals,
    /// strings, variables, arithmetic operators, and logical operators. 'Variables' are names in the expression, they
    /// are indexed during expression evaluation from a given table. 
    /// </summary>
    /// <param name="tokens">List of tokens to parse.</param>
    /// <returns>Parsed expression.</returns>
    /// <exception cref="SimpleExpressionParsingException">Thrown if the expression is malformed.</exception>
    public static SimpleExpression Parse(IEnumerable<LuaToken> tokens)
    {
        // Flag to track if we should see a value next. If we find something that isn't a value when we expect one,
        // then the operator must be a unary operator; this allows us to distinguish unary and binary negation.
        var expectingValue = true;

        var output = new Queue<SimpleExpression>();
        var operatorStack = new Stack<LuaToken>();
        
        foreach (var tokenOriginal in tokens)
        {
            var token = tokenOriginal;
            
            if (IsValueToken(token.Type))
            {
                if (!expectingValue) 
                    throw new SimpleExpressionParsingException(
                        $"Unexpected value {(token.Type is TokenType.String ? "<string>" : token.OriginalString)} " +
                        $"at line {token.StartLine}, col {token.StartCol}");
                
                // After seeing a value, we expect the next token to not be a value
                expectingValue = false;

                SimpleExpression newExp = token.Type switch
                {
                    TokenType.Name => new Simexp_Variable((string)token.Data!),
                    TokenType.String => new Simexp_Value((string)token.Data!),
                    _ => new Simexp_Value(new LuaValue(token))
                };
                
                output.Enqueue(newExp);

                continue;
            }
            
            // Special case: OpenExp '(' can be treated like a value, except we still expect a value after seeing it.
            if (token.Type == TokenType.OpenExp)
            {
                if (!expectingValue)
                    throw new SimpleExpressionParsingException(
                        $"Unexpected token {token.OriginalString} at line {token.StartLine}, col {token.StartCol}");
                
                operatorStack.Push(token);

                continue;
            }
            
            // Token is not value, parse it as an operator
            
            if (expectingValue)
            {
                // If we were expecting a value, getting an operator is invalid, unless it's a unary operator.
                if (!Grammar.UnaryOperators.Contains(token.Type))
                    throw new SimpleExpressionParsingException(
                        $"Unexpected token {token.OriginalString} at line {token.StartLine}, col {token.StartCol}");

                if (token.Type == TokenType.Sub)
                    token = token with { Type = TokenType.Neg };
                else if (token.Type == TokenType.BitwiseXor)
                    token = token with { Type = TokenType.BitwiseNot };
            }
            
            if (token.Type == TokenType.CloseExp)
            {
                while (true)
                {
                    if (operatorStack.Count == 0)
                        throw new SimpleExpressionParsingException(
                            $"Unbalanced closing parenthesis at line {token.StartLine}, col {token.StartCol}");

                    var topToken = operatorStack.Pop();

                    // Pop operators and put them in the output queue until we hit the opening parenthesis that this
                    // closing parenthesis is closing
                    if (topToken.Type != TokenType.OpenExp)
                        output.Enqueue(GetExpressionForOperator(topToken));
                    else break;
                }

                // After parsing a closing parenthesis, we expect there to not be a value still
                continue;
            }

            while (operatorStack.TryPeek(out var topToken) && topToken.Type != TokenType.OpenExp)
            {
                var curPrec = Grammar.OperatorPrecedence[token.Type];
                var topPrec = Grammar.OperatorPrecedence[topToken.Type];
                
                // 'curPrec >= 10' means 'is the current operator right-associative', since precedence is > 9 if and
                // only if the operator is right-associative.
                if (topPrec <= curPrec && (topPrec != curPrec || curPrec >= 10)) break;
                
                // Current precedence is less than top precedence, or they're the same and current has priority due to
                // associativity, pop the top operator into the output queue.
                output.Enqueue(GetExpressionForOperator(operatorStack.Pop()));
            }
            
            operatorStack.Push(token);
            
            // After parsing an operator, we expect the next token to be a value.
            expectingValue = true;
        }

        foreach (var token in operatorStack)
        {
            if (token.Type == TokenType.OpenExp)
                throw new SimpleExpressionParsingException(
                    $"Unbalanced opening parenthesis at line {token.StartLine}, col {token.StartCol}");
            
            output.Enqueue(GetExpressionForOperator(token));
        }

        if (output.Count == 0)
            throw new SimpleExpressionParsingException("Cannot parse empty expression");
        
        var initStack = new Stack<SimpleExpression>(output);
        try
        {
            var top = initStack.Pop();
            top.Init(initStack);
            return top;
        }
        catch (Exception)
        {
            throw new SimpleExpressionParsingException("Failed to initialize expression after parsing");
        }
    }

    private static bool IsValueToken(TokenType type) 
        => type is TokenType.Numeral or TokenType.String or TokenType.Name;

    private static SimpleExpression GetExpressionForOperator(LuaToken token)
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        => token.Type switch
        {
            TokenType.Add => new Simexp_OpAdd(),
            TokenType.Sub => new Simexp_OpSub(),
            TokenType.Mul => new Simexp_OpMul(),
            TokenType.Div => new Simexp_OpDiv(),
            TokenType.IntDiv => new Simexp_OpIntDiv(),
            _ => throw new SimpleExpressionParsingException(
                $"Unexpected token {token.OriginalString} when trying to parse operator on line {token.StartLine}, " +
                $"col {token.StartCol}")
        };
}