/// <summary>
/// Throw when expected term was not found
/// </summary>
public class TermException : Exception{
    /// <summary>
    /// Line index where term was expected but not found
    /// </summary>
    public int LineIndex{get;}
    /// <summary>
    /// Term that was expected
    /// </summary>
    public string TermName { get; }
    public TermException(string message,string termName,int lineIndex) : base(message){
        LineIndex = lineIndex;
        TermName = termName;
    }

}