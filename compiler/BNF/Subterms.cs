public struct Subterms{
    public Subterms()
    {
    }

    /// <summary>
    /// Left subterm of this term, so this term follows after LeftTerm
    /// </summary>
    public Term? LeftTerm{get;set;} = null;
    /// <summary>
    /// Right subterm of this term, so RightTerm follows after this term
    /// </summary>
    public Term? RightTerm{get;set;} = null;
    /// <summary>
    /// If current term is OR term that contains a lot of other terms inside of it, they're all gonna be here
    /// </summary>
    public IEnumerable<Term>? OrSubterms{get;set;} = null;
}
