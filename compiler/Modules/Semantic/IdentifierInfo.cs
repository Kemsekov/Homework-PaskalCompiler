namespace Modules;
/// <summary>
/// Information about each identifier.
/// </summary>
public struct IdentifierInfo{
    /// <summary>
    /// Name as it appears in the code
    /// </summary>
    public string Name;
    /// <summary>
    /// Type of identifier
    /// </summary>
    public IdentifierType IdentifierType;
    /// <summary>
    /// If identifier is variable or constant, what type it is
    /// </summary>
    public string? VariableType;
    /// <summary>
    /// If identifier is procedure what return type of it
    /// </summary>
    public string? ReturnType;
    /// <summary>
    /// If identifier is function or procedure what argument types are
    /// </summary>
    public string[]? Args;
    /// <summary>
    /// If identifier is constant what it's value is
    /// </summary>
    public string? ConstantValue;
}