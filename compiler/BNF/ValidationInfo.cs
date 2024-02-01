
/// <summary>
/// Validation info in calls of <see cref="Term.Validate"/> 
/// </summary>
public class ValidationInfo{
    /// <summary>
    /// Part that was validated in latest call of <see cref="Term.Validate"/>, or empty string if validation was unsuccessful
    /// </summary>
    public string LastValidatedPart => lastValidatedString[lastValidatedStartIndex..lastValidatedEndIndex].Trim(BNF.Whitespaces);
    /// <summary>
    /// Term that did validation
    /// </summary>
    public Term Term=>term;
    /// <summary>
    /// Subterms that contains further validated info
    /// </summary>
    public Subterms? Subterms=>subterms;
    int lastValidatedStartIndex = 0;
    int lastValidatedEndIndex = 0;
    string lastValidatedString = "";
    Subterms? subterms;
    Term term;
    public ValidationInfo(Term term,int startIndex,int endIndex,string str,Subterms? subterms)
    {
        lastValidatedStartIndex=startIndex;
        lastValidatedEndIndex=endIndex;
        lastValidatedString=str;
        this.subterms=subterms;
        this.term=term;
    }
    public void Update(Term term,int startIndex,int endIndex,string str,Subterms? subterms){
        lastValidatedStartIndex=startIndex;
        lastValidatedEndIndex=endIndex;
        lastValidatedString=str;
        this.subterms=subterms;
        this.term=term;
    }
    public void Update(ValidationInfo info){
        lastValidatedStartIndex=info.lastValidatedStartIndex;
        lastValidatedEndIndex=info.lastValidatedEndIndex;
        lastValidatedString=info.lastValidatedString;
        this.subterms=info.subterms;
        this.term=info.term;
    }
    public ValidationInfo Clone(){
        return new(Term,lastValidatedStartIndex,lastValidatedEndIndex,lastValidatedString,subterms);
    }
}
