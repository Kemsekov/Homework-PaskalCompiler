namespace Modules.Semantic;

public static class IdentifierInfoExtensions
{
    public static bool IsVariable(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.VARS) == IdentifierType.VARS;
    public static bool IsFunction(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.FUNCS) == IdentifierType.FUNCS;
    public static bool IsProcedure(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.PROCS) == IdentifierType.PROCS;
    public static bool IsConstant(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.CONSTS) == IdentifierType.CONSTS;
    public static bool IsType(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.TYPES) == IdentifierType.TYPES;
    public static bool IsProgram(this IdentifierInfo t)
        => (t.IdentifierType & IdentifierType.PROGS) == IdentifierType.PROGS;
}
