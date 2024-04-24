using Modules.Semantic;

namespace Modules.Nodes;
public class SimpleExpression : BaseNode, ITypedNodeTerm
{
    /// <summary>
    /// Simple expression type
    /// </summary>
    public IVariableType? Type{get;set;}
    public SimpleExpression(INode parent) : base(parent)
    {
    }
}