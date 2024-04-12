using Modules.Semantic;

namespace Modules.Nodes;

public interface ITypedNodeTerm : INode, ITypedTerm{

}

public interface ITypedTerm
{
    IVariableType? Type{get;}
}
