using Modules.Semantic;

namespace Modules.Nodes;
public class Term : BaseNode, ITypedNodeTerm
{
    /// <summary>
    /// Term type
    /// </summary>
    public IVariableType? Type{get;set;}
    public Term(INode parent) : base(parent)
    {
    }
}