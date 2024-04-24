using Modules.Semantic;

namespace Modules.Nodes;
public class ConstantWithoutSignCall : BaseNode, ITypedNodeTerm
{

    public ConstantWithoutSignCall(INode parent) : base(parent)
    {
    }

    public IVariableType? Type{get;set;}
}