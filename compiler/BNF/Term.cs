public class Term
{
    /// <summary>
    /// Parent term. It is used when new term does not add much logic but wraps around old term, like brackets {oldTerm}
    /// </summary>
    public Term? Parent = null;
    /// <summary>
    /// Term name
    /// </summary>
    public string Name { get; protected set; }
    /// <summary>
    /// Information about validation in latest call of <see cref="Validate(string)"/>
    /// </summary>
    public Matches Matches{get;protected set;}
    public Subterms? Subterms{get;protected set;} = null;
    Func<string, int, int> validate;
    /// <param name="name">Term name</param>
    /// <param name="validate">Reads string from index and returns length of validated symbol</param>
    public Term(string name, Func<string, int, int> validate)
    {
        this.validate = validate;
        Name = name;
        Matches= new(this,0,0,"",null);
    }
    /// <param name="name">Term name</param>
    /// <param name="validate">returns validated substring from the beginning of input string</param>
    public Term(string name, Func<string, string> validate)
    {
        this.validate = (s, index) => validate(s[index..]).Length;
        Name = name;
        Matches= new(this,0,0,"",null);
    }
    ///<inheritdoc cref="Term.OfSelf(Func{Term, Term})"/>
    public static Term OfSelf_(Func<Term, Term> termCreation)
    {
        var t = new Term("", s => s);
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
    public Term OfSelf(Func<Term, Term> termCreation)
    {
        var self = this;
        var term = termCreation(self);
        self.validate = term.validate;
        self.Matches = term.Matches;
        self.Subterms = term.Subterms;
        self.Name=term.Name;
        return term;
    }
    /// <summary>
    /// Creates a term that is one of constants
    /// </summary>
    public static Term OfMany(string termName, string[] constants)
    {
        var terms = constants.Select(OfConstant).ToArray();
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
            throw new TermException($"Index {index} expected '{constant}'", constant, index)
        );
    }
    public static implicit operator Term(string s){
        return Term.OfConstant(s);
    }
    public Term WithName(string name)
    {
        this.Name = name;
        return this;
    }
    public Term Follows(string constant)
    {
        return Follows(Term.OfConstant(constant));
    }

    /// <summary>
    /// t2 term follows after t1
    /// </summary>
    public Term Follows(Term t)
    {
        var name = this.Name + t.Name;
        var left = Matches.Clone();
        var right = t.Matches.Clone();
        return new(name, (s, index) =>
        {
            var validatedLength = 0;
            try{
                validatedLength += Validate(s, index);
                left.Update(Matches);
            }
            catch (Exception e){
                throw new TermException($"Index {index} Expected '{Name}'\n{e.Message}", Name, index);
            }
            
            try{
                index+=validatedLength;
                validatedLength += t.Validate(s, index);
                right.Update(t.Matches);
            }
            catch (Exception e){
                throw new TermException($"Index {index} Expected '{t.Name}'\n{e.Message}", t.Name, index);
            }

            return validatedLength;
        }
        )
        {
            Subterms=new()
            {
                LeftTerm = left,
                RightTerm = right
            }
            
        };
    }
    public static Term operator &(Term t1, Term t2){
        return t1.Follows(t2);
    }
    /// <summary>
    /// Zero or many of term t
    /// </summary>
    public Term ZeroOrMany()
    {
        var name = BNF.ZeroOrManyOpening + Name + BNF.ZeroOrManyClosing;
        var added = new List<Matches>();
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
                        added.Add(Matches.Clone());
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
            Parent=this,
            Subterms=new()
            {
                ZeroOrManyLastValidatedPart=added
            }
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
        var orTermsMatches = orTerms.Select(t=>t.Matches).ToList();
        var exceptionOrTermsNames = string.Join(",\n", orTerms);
        return new(name, (s, index) =>
        {
            orTermsMatches.Clear();
            int validatedLength = -1;
            var count = orTerms.Count;
            for(int i = 0;i<count;i++)
            {
                var t = orTerms[i];
                if (validatedLength != -1) break;
                try
                {
                    validatedLength = t.Validate(s, index);
                    orTermsMatches.Add(t.Matches.Clone());
                }
                catch { }
            }
            if (validatedLength == -1)
            {
                throw new TermException($"Index {index} expected any of\n{exceptionOrTermsNames}\n", name, index);
            }
            return validatedLength;
        }
        )
        {
            Subterms=new()
            {
                OrSubterms = orTermsMatches
            }
        };
    }
    public static Term operator |(Term t1, Term t2){
        return t1.Or(t2);
    }
    /// <summary>
    /// Validates a string
    /// </summary>
    public int Validate(string input, int index = 0)
    {
        Matches.Update(this,0,0,"",null);
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

        Matches.Update(this,index,validatedLength,input,Subterms?.DeepCopy());

        return validatedLength + skipped;
    }
    public override string ToString()
    {
        return Name;
    }
}