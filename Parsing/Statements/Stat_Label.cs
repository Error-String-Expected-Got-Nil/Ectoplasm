namespace Ectoplasm.Parsing.Statements;

public class Stat_Label(string labelName, ushort line, ushort col) : Statement(line, col)
{
    public override string ToString() => base.ToString() + $" <{labelName}>";
}