using Modules.Semantic;

namespace Modules.Nodes;
public class Variable : BaseNode, ITypedNodeTerm
{
    
    /// <summary>
    /// Variable type
    /// </summary>
    public IVariableType? Type{get;set;}
    public Variable(INode parent) : base(parent)
    {
    }
}