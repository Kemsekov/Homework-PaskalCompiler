using System.Collections.Concurrent;
using Modules;

public static class Keywords
{
    public static byte MaxKeywordLength;
    public static byte MinKeywordLength { get; }

    /// <summary>
    /// Key is keyword length, then in resulting dictionary key is keyword name, value is it's code
    /// </summary>
    public static ConcurrentDictionary<byte, ConcurrentDictionary<string, byte>> Kw{get;private set;}
    /// <summary>
    /// Inverse keywords mapping from code to keyword string
    /// </summary>
    public static ConcurrentDictionary<byte, string> InverseKw{get;private set;}

    static Keywords()
    {
        Kw = new ConcurrentDictionary<byte, ConcurrentDictionary<string, byte>>();
        InverseKw = new ConcurrentDictionary<byte, string>();

        var tmp = new ConcurrentDictionary<string, byte>();

        tmp["*"] = Lexical.star;
        tmp["/"] = Lexical.slash;
        tmp["="] = Lexical.equal;
        tmp[","] = Lexical.comma;
        tmp[";"] = Lexical.semicolon;
        tmp[":"] = Lexical.colon;
        tmp["."] = Lexical.point;
        tmp["^"] = Lexical.arrow;
        tmp["("] = Lexical.leftpar;
        tmp[")"] = Lexical.rightpar;
        tmp["["] = Lexical.lbracket;
        tmp["]"] = Lexical.rbracket;
        tmp["{"] = Lexical.flpar;
        tmp["}"] = Lexical.frpar;
        tmp["<"] = Lexical.later;
        tmp[">"] = Lexical.greater;
        tmp["+"] = Lexical.plus;
        tmp["-"] = Lexical.minus;
        Kw[1] = tmp;
        
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["do"] = Lexical.dosy;
        tmp["if"] = Lexical.ifsy;
        tmp["in"] = Lexical.insy;
        tmp["of"] = Lexical.ofsy;
        tmp["or"] = Lexical.orsy;
        tmp["to"] = Lexical.tosy;
        tmp["<="] = Lexical.laterequal;
        tmp[">="] = Lexical.greaterequal;
        tmp["<>"] = Lexical.latergreater;
        tmp["(*"] = Lexical.lcomment;
        tmp["*)"] = Lexical.rcomment;
        tmp[":="] = Lexical.assign;
        tmp[".."] = Lexical.twopoints;
        Kw[2] = tmp;

        tmp = new ConcurrentDictionary<string, byte>();
        tmp["end"] = Lexical.endsy;
        tmp["var"] = Lexical.varsy;
        tmp["div"] = Lexical.divsy;
        tmp["and"] = Lexical.andsy;
        tmp["not"] = Lexical.notsy;
        tmp["for"] = Lexical.forsy;
        tmp["mod"] = Lexical.modsy;
        tmp["nil"] = Lexical.nilsy;
        tmp["set"] = Lexical.setsy;
        Kw[3] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["then"] = Lexical.thensy;
        tmp["else"] = Lexical.elsesy;
        tmp["case"] = Lexical.casesy;
        tmp["file"] = Lexical.filesy;
        tmp["goto"] = Lexical.gotosy;
        tmp["type"] = Lexical.typesy;
        tmp["with"] = Lexical.withsy;
        Kw[4] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["begin"] = Lexical.beginsy;
        tmp["while"] = Lexical.whilesy;
        tmp["array"] = Lexical.arraysy;
        tmp["const"] = Lexical.constsy;
        tmp["label"] = Lexical.labelsy;
        tmp["until"] = Lexical.untilsy;
        Kw[5] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["downto"] = Lexical.downtosy;
        tmp["packed"] = Lexical.packedsy;
        tmp["record"] = Lexical.recordsy;
        tmp["repeat"] = Lexical.repeatsy;
        Kw[6] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["program"] = Lexical.programsy;
        Kw[7] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["function"] = Lexical.functionsy;
        Kw[8] = tmp;
        tmp = new ConcurrentDictionary<string, byte>();
        tmp["procedure"] = Lexical.proceduresy;
        Kw[9] = tmp;
        MaxKeywordLength=9;
        MinKeywordLength = 1;

        foreach(var kw in Kw){
            foreach(var k in kw.Value){
                InverseKw[k.Value]=k.Key;
            }
        }
        InverseKw[Lexical.floatc]="floatc";
        InverseKw[Lexical.stringc]="stringc";
        InverseKw[Lexical.intc]="intc";
        InverseKw[Lexical.ident]="ident";
        InverseKw[Lexical.undefined]="undefined";
    }
    
    /// <summary>
    /// Searches string constant in a word
    /// </summary>
    /// <returns>symbol if word is legal string constant, null + errorCode if not</returns>
    public static byte SearchStringConstant(string word,out long errorCode){
        errorCode=-1;
        if(word.Length>1 && word[0]=='\'' && word[^1]=='\''){
            if(word.Count(c=>c=='\'')==2)
                return Lexical.stringc;
        }
        errorCode=(long)ErrorCodes.StringFormatting;
        return Lexical.undefined; //wrong string formatting
    }

    /// <summary>
    /// Searches variable name in a word
    /// </summary>
    /// <returns>symbol if word is legal variable name, null + errorCode if not</returns>
    public static byte SearchVariable(string word,out long errorCode){
        errorCode=-1;
        //the only variants left is ident, floatc and intc
        //if first char is word, assume it is ident
        if(char.IsLetter(word[0])){
            if(word.All(c=>char.IsLetterOrDigit(c) || c=='_'))
                return Lexical.ident;
        }
        errorCode=(long)ErrorCodes.WrongVariableName;
        return Lexical.undefined; //wrong variable name formatting. Must have letters, digits and _
    }
    /// <summary>
    /// Searches float constant in a word
    /// </summary>
    /// <returns>symbol if word is legal float constant, null + errorCode if not</returns>
    public static byte SearchFloatConstant(string word,out long errorCode){
        errorCode=-1;
        var formatting = word.All(c=>c=='.' || char.IsDigit(c));
        var dotCount = word.Count(c=>c=='.')==1;

        if(!formatting){
            errorCode=(long)ErrorCodes.UnrecognizedSymbol;
            return Lexical.undefined; //unrecognized symbol
        }

        if(!dotCount){
            errorCode= (long)ErrorCodes.FloatConstantError;
            return Lexical.undefined; //floating point must have one dot symbol .
        }
        return Lexical.floatc;
    }
    /// <summary>
    /// Searches int constant in a word
    /// </summary>
    /// <returns>symbol if word is legal int constant, null + errorCode if not</returns>
    public static byte SearchIntConstant(string word,out long errorCode){
        errorCode=-1;
        var formatting = word.All(char.IsDigit);
        if(!formatting){
            errorCode= (long)ErrorCodes.UnrecognizedSymbol;
            return Lexical.undefined; //unrecognized symbol
        }
        return Lexical.intc;
    }

    // TODO: add test
    /// <summary>
    /// Searches for keyword code of given word
    /// </summary>
    /// <returns>symbol if word is legal keyword, null + errorCode if not</returns>
    public static byte SearchKeyword(string word,out long errorCode){
        errorCode=-1;
        if(word.Length==0) return Lexical.undefined;

        if(word.Length <= MaxKeywordLength && word.Length >= MinKeywordLength)
        if(Kw[(byte)word.Length].TryGetValue(word,out var code))
            return code;
        
        errorCode= (long)ErrorCodes.ForbiddenSymbol; //forbidden symbol
        return Lexical.undefined;
    }
}