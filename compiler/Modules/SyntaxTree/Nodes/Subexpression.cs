using Modules.Semantic;

namespace Modules.Nodes;
/// <summary>
/// Subexpression in a form like (expression)
/// </summary>
public class Subexpression : BaseNode, ITypedNodeTerm
{
    public IVariableType? Type{get;set;}
    public Subexpression(INode parent) : base(parent)
    {
    }
}