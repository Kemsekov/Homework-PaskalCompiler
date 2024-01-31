

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Mvc;

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

public class Term
{
    /// <summary>
    /// Term name
    /// </summary>
    public string Name { get; protected set; }
    /// <summary>
    /// Part that was validated in latest call of <see cref="Validate(string)"/>, or empty string if validation was unsuccessful
    /// </summary>
    public string LastValidatedPart { get; protected set; } = "";
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
            throw new Exception($"'{constant}' expected")
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
                throw new Exception($"Expected term {name} not found\n{e.Message}");
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
    public Term Or(params Term[] terms)
    {
        var name = Name;
        foreach (var t in terms)
        {
            name += BNF.Or + t.Name;
        }
        var orTerms = terms.Append(this).ToList();
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
                throw new Exception($"None of {name} found on '{s}'");
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
        LastValidatedPart = "";
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
        if(Name=="likes|wants"){
            System.Console.WriteLine('a');
        }
        var validatedLength = validate(input, index);
        LastValidatedPart = input[index..(index + validatedLength)].Trim(BNF.Whitespaces);

        return validatedLength + skipped;
    }
    public override string ToString(){
        return Name;
    }
}

public class BNF
{
    // definitions for BNF symbols
    public static string CommentOpening = "(*";
    public static string CommentClosing = "*)";
    public static char NonterminalSymbolOpening = '<';
    public static char NonterminalSymbolClosing = '>';
    public static char Or = '|';
    public static char ZeroOrManyOpening = '{';
    public static char ZeroOrManyClosing = '}';
    public static string Equality = "::=";
    /// <summary>
    /// separates different symbols definition
    /// </summary>
    public static char Delimiter = '\n';
    /// <summary>
    /// Whitespace that are ignored by bnf
    /// </summary>
    public static char[] Whitespaces = [' ', '\n'];
    public BNF(string bnf) : this(bnf.Split(Delimiter)) { }
    public BNF(string[] lines)
    {
        var clearedLines = lines
        .Select(line => line.Trim())  //trim from newlines and whitespaces
        .Where(line => line.Length > 0) //skip empty
        .Where(line => !(line.StartsWith(CommentOpening) && line.EndsWith(CommentClosing))); //skip comments

        var definitions = new Dictionary<string, string>();
        foreach (var line in clearedLines)
        {
            int i = 0;
            //search symbol name

            string name = SearchNonterminalSymbolName(line, ref i);
            if (line[i..(i + Equality.Length)] != Equality)
            {
                throw new Exception($"Each nonterminal symbol definition must have '{Equality}' at line\n{line}");
            }
            i += Equality.Length;

            var definition = line[i..];
            definitions[name] = definition;
        }
    }
    //search substring that contains block like '{...}'
    string SearchZeroOrManyBlock(string line, ref int i)
    {
        while (i < line.Length)
            if (line[i++] == ZeroOrManyOpening) break;
        var startI = i;
        for (; i < line.Length; i++)
        {
            var c = line[i];
            if (c == ZeroOrManyClosing)
                break;
        }
        if (line[i++] != ZeroOrManyClosing)
            throw new Exception($"Zero or many block must end with '{ZeroOrManyClosing}' at line\n{line}");

        return line[startI..i];
    }
    string SearchNonterminalSymbolName(string line, ref int i)
    {
        if (line[i++] != NonterminalSymbolOpening)
            throw new Exception($"Nonterminal symbol must start with '{NonterminalSymbolOpening}' at line\n{line}");

        for (; i < line.Length; i++)
        {
            var c = line[i];
            if (char.IsLetter(c) || c == '-') continue;
            break;
        }

        var name = line[1..i];

        if (line[i++] != NonterminalSymbolClosing)
            throw new Exception($"Nonterminal symbol must end with '{NonterminalSymbolClosing}' at line\n{line}");
        return name;
    }

    /// <summary>
    /// Adds basic symbol definition. <br/>
    /// This method can be used to define nonterminal symbol of letter, 
    /// digit, plus or minus signs, etc.
    /// </summary>
    /// <param name="nonterminalSymbol">what to define</param>
    /// <param name="definition">
    /// Function that takes string and returns valid substring 
    /// of it from the beginning of input 
    /// string and throws if input string do not contain 
    /// valid sequence in the beginning<br/>
    /// For example valid equality '==' symbol definition: <br/>
    /// str=>str.TakeWhile(c=>c=='=').Count()==2 ? "==" : throw new Exception("expected equality sign '=='");
    /// </param>
    public void AddBasicSymbol(string nonterminalSymbol, Func<string, string> definition)
    {

    }
}