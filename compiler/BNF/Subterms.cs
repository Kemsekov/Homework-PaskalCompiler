/// <summary>
/// Term subterms
/// </summary>
public struct Subterms{
    public Subterms()
    {
    }
    /// <summary>
    /// Validation of left subterm of this term, so this term follows after LeftTerm
    /// </summary>
    public Matches? LeftTerm= null;
    /// <summary>
    /// Validation of right subterm of this term, so RightTerm follows after this term
    /// </summary>
    public Matches? RightTerm = null;
    /// <summary>
    /// If current term is OR term that contains a lot of other terms inside of it, their validation results gonna be here
    /// </summary>
    public IList<Matches> OrSubterms = [];
    /// <summary>
    /// Zero or many calls validated parts on last validate call
    /// </summary>
    public IList<Matches> ZeroOrManyLastValidatedPart = [];
    public Subterms DeepCopy(){
        var newSubt = new Subterms
        {
            LeftTerm = LeftTerm?.Clone(),
            RightTerm = RightTerm?.Clone(),
            OrSubterms=OrSubterms.Select(s=>s.Clone()).ToList(),
            ZeroOrManyLastValidatedPart=ZeroOrManyLastValidatedPart.Select(s=>s.Clone()).ToList()
        };
        return newSubt;

    }
}
