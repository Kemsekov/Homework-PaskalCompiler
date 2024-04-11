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
    /// <summary>
    /// Composition operation that will be applied to all children of given node
    /// </summary>
    void Operation(Action<INode> action);
}
