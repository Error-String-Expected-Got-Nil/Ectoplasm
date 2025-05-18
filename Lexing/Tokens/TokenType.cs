namespace Ectoplasm.Lexing.Tokens;

/// <summary>
/// All types of code tokens present in Lua.
/// </summary>
public enum TokenType
{
    None,
    
    // Keywords
    Do,
    End,
    If,
    Then,
    Elseif,
    Else,
    For,
    In,
    While,
    Repeat,
    Until,
    Goto,
    Function,
    Local,
    Break,
    Return,
    
    // Logical keywords
    Nil,
    False,
    True,
    And,
    Or,
    Not,
    
    // Delimiter symbols
    Statement, // Semicolon ;
    Separator, // Comma ,
    Varargs, // Ellipses ...
    LabelSep, // Double colon ::
    OpenExp, // Open parenthesis (
    CloseExp, // Close parenthesis )
    OpenIndex, // Open bracket [
    CloseIndex, // Close bracket ]
    OpenTable, // Open curly bracket {
    CloseTable, // Close curly bracket }
    Comment, // Double hyphen --
    
    // Operator symbols
    Add, // '+'
    Sub, // '-' (Also unary minus)
    Mul, // '*'
    Div, // '/'
    IntDiv, // '//' (Integer/floor division)
    Exp, // '^' (Power/exponentiation)
    Mod, // '%'
    BitwiseAnd, // '&'
    BitwiseXor, // '~' (Also unary bitwise not)
    BitwiseOr, // '|'
    ShiftRight, // '>>'
    ShiftLeft, // '<<'
    Concat, // '..'
    LessThan, // '<'
    LessThanOrEq, // '<='
    GreaterThan, // '>'
    GreaterThanOrEq, // '>='
    EqualTo, // '=='
    NotEqualTo, // '~='
    IndexName, // '.'
    IndexMethod, // ':'
    
    // Value tokens
    Name, // Identifiers
    Numeral, // Numbers, both float and integer
    String, // String literals
    Whitespace // Whitespace
}