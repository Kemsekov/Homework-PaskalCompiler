

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Mvc;

public class Term
{
    /// <summary>
    /// Term name
    /// </summary>
    public string Name { get; protected set; }
    Func<string, string> validate;
    public Term(string name, Func<string, string> validate)
    {
        this.validate = validate;
        Name = name;
    }
    public static Term OfMany(string termName, string[] constants)
    {
        var terms = constants.Select(v => OfConstant(v, v)).ToArray();
        var res =  terms[0].Or(terms[1..]);
        res.Name=termName;
        return res;
    }
    public static Term OfConstant(string termName, string constant)
    {
        return new(termName,
            s =>
            s[0..constant.Length] == constant ? s[constant.Length..] :
            throw new Exception($"'{constant}' expected on line '{s}'")
        );
    }
    /// <summary>
    /// t2 term follows after t1
    /// </summary>
    public Term Follows(Term t)
    {
        var name = this.Name + t.Name;
        return new(name, s =>
        {
            var originalS = s;
            try
            {
                s = Validate(s);
                s = t.Validate(s);
                return s;
            }
            catch
            {
                throw new Exception($"Expected term {name} not found on '{originalS}'");
                throw;
            }
        }
        );
    }
    /// <summary>
    /// Zero or many of term t
    /// </summary>
    public Term ZeroOrMany()
    {
        var name = BNF.ZeroOrManyOpening + Name + BNF.ZeroOrManyClosing;
        return new(name, s =>
        {
            while (true)
                try
                {
                    s = Validate(s);
                }
                catch
                {
                    break;
                }
            return s;
        }
        );
    }
    public Term Or(params Term[] terms)
    {
        var name = Name;
        foreach (var t in terms)
        {
            name += BNF.Or + t.Name;
        }
        return new(name, s =>
        {
            var originalS = s;
            string? res = null;
            foreach (var t in terms.Append(this))
            {
                if (res is not null) break;
                try
                {
                    res = t.Validate(s);
                }
                catch { }
            }
            if (res is null)
            {
                throw new Exception($"Expected one of {name} not found on '{originalS}'");
            }
            return res;
        }
        );
    }
    public Term Or(Term t)
    {
        var name = Name + BNF.Or + t.Name;
        return new(name, s =>
        {
            var originalS = s;
            string? res = null;
            try
            {
                res = Validate(s);
            }
            catch { }
            if (res is null)
                try
                {
                    res = t.Validate(s);
                }
                catch { }

            if (res is null)
            {
                throw new Exception($"Expected one of {name} not found on '{originalS}'");
            }
            return res;
        }
        );
    }
    public string Validate(string input)
    {
        input = input.Trim(BNF.Whitespaces);
        return validate(input).Trim(BNF.Whitespaces);
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