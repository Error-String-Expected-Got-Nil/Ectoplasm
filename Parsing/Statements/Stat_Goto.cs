namespace Ectoplasm.Parsing.Statements;

public class Stat_Goto(string targetLabel, ushort line, ushort col) : Statement(line, col)
{
    public override string ToString() => base.ToString() + $" <{targetLabel}>";
}