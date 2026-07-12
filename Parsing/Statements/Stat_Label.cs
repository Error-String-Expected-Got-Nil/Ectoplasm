namespace Ectoplasm.Parsing.Statements;

public class Stat_Label(string labelName, ushort line, ushort col) : Statement(line, col)
{
    public override bool IsVoid => true;
    
    public string LabelName => labelName;
    
    public override string ToString() => base.ToString() + $" <{labelName}>";
}