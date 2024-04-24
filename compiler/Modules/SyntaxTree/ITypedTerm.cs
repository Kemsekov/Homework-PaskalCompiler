using Modules.Semantic;

namespace Modules.Nodes;

public interface ITypedNodeTerm : INode{
    IVariableType? Type{get;}

}

