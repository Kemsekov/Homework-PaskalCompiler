
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