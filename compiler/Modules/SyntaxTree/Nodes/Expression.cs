using Modules.Semantic;

namespace Modules.Nodes;
public class Expression(INode parent) : BaseNode(parent), ITypedTerm
{
    /// <summary>
    /// Expression type
    /// </summary>
    public IVariableType? Type{get;set;}
}