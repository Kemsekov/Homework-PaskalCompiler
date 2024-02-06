
/// <summary>
/// Class that allows for recursive terms creation
/// </summary>
public class RecursiveTermsCreation{
    public RecursiveTermsCreation()
    {
        Terms = new Dictionary<string,Term>();
        TermsFactory = new Dictionary<string,Func<Term>>();
    }
    IDictionary<string,Term> Terms{get;}
    IDictionary<string,Func<Term>> TermsFactory{get;}
    /// <summary>
    /// Get term by name
    /// </summary>
    public Term this[string termName]{
        get{
            if(!TermsFactory.ContainsKey(termName))
                throw new KeyNotFoundException("Cannot find term with name "+termName);
            
            if(Terms.ContainsKey(termName)) return Terms[termName];

            // return Terms[termName]=TermsFactory[termName]();
            return Term.OfSelf_(t=>{
                Terms[termName] = t.WithName(termName);
                return TermsFactory[termName]();    
            });
        }
    }
    /// <summary>
    /// Add term creation function. <br/>
    /// You will need to use current object <see cref="this[string]"/> to get any subterms in creation method
    /// </summary>
    public void Add(string termName,Func<Term> creation){
        TermsFactory[termName] = ()=>creation().WithName(termName);
    }
}
