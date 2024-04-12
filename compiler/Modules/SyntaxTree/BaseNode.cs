using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Modules.Nodes;
public record Token(byte Symbol, string Value, TextPosition TextPosition);
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
    public void Operation(Action<INode> action){
        foreach(var c in children){
            c.Operation(action);
        }
        action(this);
    }
}

public static class NodeExtensions{
    /// <returns>Tokens that corresponds to inner</returns>
    public static Token[] Tokens(this INode node){
        var tokens = new List<Token>();
        node.Operation(n=>{
            if(n is not Accept acn) return;
            tokens.Add(new Token(acn.Symbol,acn.Value,acn.Pos));
        });
        return tokens.ToArray();
    }
}
