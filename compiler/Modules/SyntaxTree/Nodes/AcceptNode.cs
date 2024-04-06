namespace Modules.Nodes;
public class Accept : BaseNode
{
    public Accept(INode parent,TextPosition pos, string value, byte symbol) : base(parent)
    {
        Pos = pos;
        Value = value;
        Symbol = symbol;
    }
    public TextPosition Pos{get;}
    public string Value{get;}
    public byte Symbol{get;}
}
