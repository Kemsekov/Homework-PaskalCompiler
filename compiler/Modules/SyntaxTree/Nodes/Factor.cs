using Modules.Semantic;

namespace Modules.Nodes;
public class Factor : BaseNode, ITypedNodeTerm
{
    /// <summary>
    /// Factor type
    /// </summary>
    public IVariableType? Type{get;set;}
    public Factor(INode parent) : base(parent)
    {
    }
}