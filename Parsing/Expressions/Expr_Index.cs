namespace Ectoplasm.Parsing.Expressions;

public class Expr_Index(ushort line, ushort col) : Expr_Binary(line, col)
{
    // OpA is table to index from
    // OpB is the key to index with
}