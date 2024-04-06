namespace Modules.Nodes;
public interface INode
{
    IEnumerable<INode> Children { get; }
    void AddChild(INode node);
    void RemoveChild(INode node);
}

public class BaseNode : INode
{
    HashSet<INode> children = new();
    public IEnumerable<INode> Children =>children;

    public void AddChild(INode node)
    {
        children.Add(node);
    }
    public void RemoveChild(INode node)
    {
        children.Remove(node);
    }
}
