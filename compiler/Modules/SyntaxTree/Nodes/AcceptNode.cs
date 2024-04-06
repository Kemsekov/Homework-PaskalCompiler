namespace Modules.Nodes;
public class AcceptNode : BaseNode
{
    public AcceptNode(TextPosition pos, string value, byte symbol)
    {
        Pos = pos;
        Value = value;
        Symbol = symbol;
    }
    public TextPosition Pos{get;}
    public string Value{get;}
    public byte Symbol{get;}
}
