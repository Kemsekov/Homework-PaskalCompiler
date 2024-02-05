
/// <summary>
/// Matches from calls of <see cref="Term.Validate"/> 
/// </summary>
public class Matches{
    /// <summary>
    /// Part that was validated in latest call of <see cref="Term.Validate"/>, or empty string if validation was unsuccessful
    /// </summary>
    public string Match => SourceString[Index..(Index+Length)].Trim(BNF.Whitespaces);
    public Matches[] SubMatches{get;protected set;} = [];
    /// <summary>
    /// Term that did validation
    /// </summary>
    public Term Term{get;protected set;}
    /// <summary>
    /// Subterms that contains separated matching values
    /// </summary>
    public Subterms? Subterms{get;protected set;}
    public int Index = 0;
    public int Length = 0;
    string SourceString = "";

    void InitMatches(){
        if(Subterms is not null){
            var s = (Subterms)Subterms;
            var ma = new List<Matches>();
            ma.AddRange(s.ZeroOrManyLastValidatedPart);
            ma.AddRange(s.OrSubterms);
            if(s.LeftTerm is not null)
                ma.Add(s.LeftTerm);
            if(s.RightTerm is not null)
                ma.Add(s.RightTerm);
            SubMatches = ma.OrderBy(m=>m.Index).Where(m=>m.Match!="").ToArray();
        }
    }
    public Matches(Term term,int index,int length,string str,Subterms? subterms)
    {
        Index=index;
        Length=length;
        SourceString=str;
        this.Subterms=subterms;
        this.Term=term;
        InitMatches();
    }
    public void Update(Term term,int index,int length,string str,Subterms? subterms){
        Index=index;
        Length=length;
        SourceString=str;
        this.Subterms=subterms;
        this.Term=term;
        InitMatches();
    }
    public void Update(Matches info){
        Index=info.Index;
        Length=info.Length;
        SourceString=info.SourceString;
        this.Subterms=info.Subterms;
        this.Term=info.Term;
        InitMatches();
    }
    public Matches Clone(){
        return new(Term,Index,Length,SourceString,Subterms);
    }
}
