using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Modules.Nodes;

public abstract class BaseNode : INode
{
    public string Name =>this.GetType().Name;
    public BaseNode(INode? parent)
    {
        Parent = parent;
    }
    List<INode> children = new();
    public IEnumerable<INode> Children =>children;

    public INode? Parent{get;}

    public void AddChild(INode node)
    {
        children.Add(node);
    }
    public void RemoveChild(INode node)
    {
        children.Remove(node);
    }
}
