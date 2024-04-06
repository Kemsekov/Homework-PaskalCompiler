namespace Modules.Nodes;
public interface INode
{
    /// <summary>
    /// Construction name
    /// </summary>
    string Name{get;}
    /// <summary>
    /// Parent node
    /// </summary>
    INode? Parent{get;}
    /// <summary>
    /// Children nodes
    /// </summary>
    IEnumerable<INode> Children { get; }
    void AddChild(INode node);
    void RemoveChild(INode node);
}
