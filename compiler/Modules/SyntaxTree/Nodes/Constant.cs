using Modules.Semantic;

namespace Modules.Nodes;
public class Constant : BaseNode, ITypedTerm
{
    /// <summary>
    /// Constant type
    /// </summary>
    public IVariableType? Type{get;set;}
    public Constant(INode parent) : base(parent)
    {
    }
}