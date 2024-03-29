namespace Modules.Semantic;

public class ArrayType : IVariableType{
    public required RangedType[] Dimensions{get;set;}
    public required IVariableType ElementType{get;set;}
}

