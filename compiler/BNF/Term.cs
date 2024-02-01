public class Term
{
    /// <summary>
    /// Term name
    /// </summary>
    public string Name { get; protected set; }
    /// <summary>
    /// Part that was validated in latest call of <see cref="Validate(string)"/>, or empty string if validation was unsuccessful
    /// </summary>
    public string LastValidatedPart => LastValidatedString[lastValidatedStartIndex..lastValidatedEndIndex].Trim(BNF.Whitespaces);
    public string LastValidatedString{get;protected set;} = "";
    public int lastValidatedStartIndex{ get; protected set; } = 0;
    public int lastValidatedEndIndex{ get; protected set; } = 0;
    /// <summary>
    /// Zero or many calls validated parts on last validate call
    /// </summary>
    public IList<string> ZeroOrManyLastValidatedPart{get;protected init;}=[];
    /// <summary>
    /// This term subterms
    /// </summary>
    public Subterms Subterms{get;protected set;}
    Func<string, int, int> validate;
    /// <param name="name">Term name</param>
    /// <param name="validate">Reads string from index and returns length of validated symbol</param>
    public Term(string name, Func<string, int, int> validate)
    {
        this.validate = validate;
        Name = name;
    }
    /// <param name="name">Term name</param>
    /// <param name="validate">returns validated substring from the beginning of input string</param>
    public Term(string name, Func<string, string> validate)
    {
        this.validate = (s, index) => validate(s[index..]).Length;
        Name = name;
    }
    ///<inheritdoc cref="Term.OfSelf(Func{Term, Term})"/>
    public static Term OfSelf_(Func<Term,Term> termCreation){
        var t = new Term("",s=>s);
        return t.OfSelf(termCreation);
    }

    /// <summary>
    /// Creates a term that can reference to itself in creation process<br/>
    /// for example: `bool expr`=`variable` `bool op` `variable` | not `bool expr`<br/>
    /// If you gonna use it with "Or" statements make sure to add 
    /// self reference to the end of the statement => so you will not get stuck
    /// into infinite recursion
    /// </summary>
    /// <param name="termCreation">Method to create a new term out of this term, making new term definition to be a definition of input term</param>
    public Term OfSelf(Func<Term,Term> termCreation){
        var self = this;
        var term = termCreation(self);
        self.validate=term.validate;
        self.Subterms=term.Subterms;
        return term;
    }
    /// <summary>
    /// Creates a term that is one of constants
    /// </summary>
    public static Term OfMany(string termName, string[] constants)
    {
        var terms = constants.Select(v => OfConstant(v)).ToArray();
        var res = terms[0].Or(terms[1..]);
        res.Name = termName;
        return res;
    }
    /// <summary>
    /// Creates a term that matches constant
    /// </summary>
    public static Term OfConstant(string constant)
    {
        return new(constant,
            (s, index) =>
            s[index..(index + constant.Length)] == constant ? constant.Length :
            throw new TermException($"'{constant}' expected on index {index}",constant,index)
        );
    }
    public Term WithName(string name){
        this.Name = name;
        return this;
    }
    public Term Follows(string constant){
        return Follows(Term.OfConstant(constant));
    }

    /// <summary>
    /// t2 term follows after t1
    /// </summary>
    public Term Follows(Term t)
    {
        var name = this.Name + t.Name;
        return new(name, (s, index) =>
        {
            var validatedLength = 0;
            try
            {
                validatedLength += Validate(s, index);
                validatedLength += t.Validate(s, index + validatedLength);
                return validatedLength;
            }
            catch (Exception e)
            {
                throw new TermException($"Expected term {name} not found on index {index}\n{e.Message}",name,index);
            }
        }
        )
        {
            Subterms=new(){
                LeftTerm=this,
                RightTerm=t
            }
        };
    }
    /// <summary>
    /// Zero or many of term t
    /// </summary>
    public Term ZeroOrMany()
    {
        var name = BNF.ZeroOrManyOpening + Name + BNF.ZeroOrManyClosing;
        var added = new List<string>();
        return new(name, (s, index) =>
        {
            var validatedLength = 0;
            added.Clear();
            while (true)
                try
                {
                    var valid = Validate(s, index + validatedLength);
                    validatedLength += valid;
                    if (valid != 0)
                        added.Add(LastValidatedPart);
                    else break;
                }
                catch
                {
                    break;
                }
            return validatedLength;
        }
        )
        {
            ZeroOrManyLastValidatedPart=added,
            Subterms=Subterms
        };
    }
    /// <summary>
    /// Adds Or terms that validated in same order as added, and validation is terminated
    /// when any of terms successfully validates input. <br/>
    /// So put longer sentences in the beginning of Or terms in order for validator to try 
    /// first longer terms and then if fails shorter ones
    /// </summary>
    public Term Or(params Term[] terms)
    {
        var name = Name;
        foreach (var t in terms)
        {
            name += BNF.Or + t.Name;
        }
        var orTerms = terms.Prepend(this).ToList();
        var exceptionOrTermsNames = string.Join(", ",orTerms);
        return new(name, (s, index) =>
        {
            int validatedLength = -1;
            foreach (var t in orTerms)
            {
                if (validatedLength != -1) break;
                try
                {
                    validatedLength = t.Validate(s, index);
                }
                catch { }
            }
            if (validatedLength == -1)
            {
                throw new TermException($"None of expected terms '{exceptionOrTermsNames}' found on index {index}",name,index);
            }
            return validatedLength;
        }
        )
        {
            Subterms=new(){
                OrSubterms=orTerms
            }
        };
    }
    /// <summary>
    /// Validates a string and cuts validated part from the beginning of the string, returning left not validated part
    /// </summary>
    public int Validate(string input, int index = 0)
    {
        LastValidatedString = "";
        var skipped = 0;
        try
        {
            while (BNF.Whitespaces.Contains(input[index]) && index < input.Length)
            {
                skipped++;
                index++;
            }
        }
        catch { }
        var validatedLength = validate(input, index);
        LastValidatedString=input;
        lastValidatedStartIndex=index;
        lastValidatedEndIndex=index + validatedLength;

        return validatedLength + skipped;
    }
    public override string ToString(){
        return Name;
    }
}
