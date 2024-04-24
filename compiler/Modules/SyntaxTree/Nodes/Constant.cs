using Modules.Semantic;

namespace Modules.Nodes;
public class Constant : BaseNode, ITypedNodeTerm
{
    /// <summary>
    /// Constant type
    /// </summary>
    public IVariableType? Type{get;set;}
    public Constant(INode parent) : base(parent)
    {
    }
}