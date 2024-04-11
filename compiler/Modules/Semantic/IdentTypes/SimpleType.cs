namespace Modules.Semantic;

public class SimpleType : IVariableType{
    public required string Name{get;set;}
    public override bool Equals(object? obj)
    {
        if(obj is SimpleType s){
            return Name==s.Name;
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}


