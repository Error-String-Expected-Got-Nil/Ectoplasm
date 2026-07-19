using System.Text;
using Ectoplasm.Lexing;
using Ectoplasm.Parsing.Expressions;
using Ectoplasm.Utils;

namespace Ectoplasm.Parsing.Statements;

/// <param name="namelist">List of loop variables. First is control variable.</param>
/// <param name="explist">Expressions that define loop parameters.</param>
public class Stat_ForGeneric(List<string> namelist, List<Expression> explist, List<Statement> block, ushort line, 
    ushort col) : Statement(line, col)
{
    // TODO: For loops should clear these locals after the loop ends to avoid keeping their contents alive longer than
    //  necessary
    // TODO: Due to the closing variable, all generic for loops will have to be wrapped in an implicit closing block
    private readonly List<LocalVariable> _visibleControls = [];
    private LocalVariable? _internalIterator;
    private LocalVariable? _internalState;
    private LocalVariable? _internalClosing;
    
    public override bool IsBreakable => true;

    public override IEnumerable<Expression> GetExpressions() => explist;

    public override IEnumerable<LocalVariable>? DeclareLocals(Prototype prototype)
    {
        foreach (var name in namelist) _visibleControls.Add(prototype.AddNewLocal(name));
        _internalIterator = prototype.AddNewLocal($"for[{StartLine},{StartCol}]iterator");
        _internalState = prototype.AddNewLocal($"for[{StartLine},{StartCol}]state");
        _internalClosing = prototype.AddNewLocal($"for[{StartLine},{StartCol}]closing", LocalAttribute.Close);
        return null;
    }

    public override IEnumerable<(List<Statement> Block, List<LocalVariable>? BlockLocals)> GetBlocks()
        => [(block, _visibleControls)];

    protected override void AddToDebugString(StringBuilder str, int depth)
    {
        base.AddToDebugString(str, depth);
        str.AppendRep(".   ", depth + 1, "Namelist:");
        foreach (var name in namelist) str.AppendRep(".   ", depth + 2, name);

        str.AppendRep(".   ", depth + 1, "Expressions:");
        for (var i = 0; i < explist.Count; i++)
        {
            var expr = explist[i];
            str.AppendRep(".   ", depth + 2, $"Expression {i}:");
            expr.AddToDebugString(str, depth + 3);
        }

        str.AppendRep(".   ", depth + 1, "Block:");
        AddBlockDebugString(str, block, depth + 2);
    }
}