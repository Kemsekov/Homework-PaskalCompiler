namespace Modules;
using static Lexical;
public class SyntaxAnalysis
{
    public SyntaxAnalysis(LexicalAnalysis lexical, InputOutput inputOutput, ErrorDescriptions errorDescriptions, ConfigurationVariables configuration)
    {
        Configuration = configuration;
        LexicalAnalysis = lexical;
        ErrorDescriptions = errorDescriptions;
        InputOutput = inputOutput;
    }
    public TextPosition Pos => LexicalAnalysis.Pos;
    public ConfigurationVariables Configuration { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public ErrorDescriptions ErrorDescriptions { get; }
    public InputOutput InputOutput { get; }
    byte Symbol => LexicalAnalysis.Symbol;
    void type()
    {
        //TODO:
        // check current symbol to be valid type, 
        //for now just accept ident
        Accept(ident);
    }
    void SimpleType()
    {
        // check that current symbol is valid simple type (int,float,string)
        if (LexicalAnalysis.SymbolValue == "int" || LexicalAnalysis.SymbolValue == "float" || LexicalAnalysis.SymbolValue == "string")
        {
            Accept(ident);
            return;
        }
        InputOutput.LineErrors().Add(
            new Error
            {
                ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                Position = LexicalAnalysis.Pos,
                SpecificErrorDescription = $"Expected type int,float or string, but {LexicalAnalysis.SymbolValue} found"
            }
        );
    }
    void variable()
    {
        Accept(ident);
        while (Symbol == lbracket || Symbol == point
        || Symbol == arrow)
            switch (Symbol)
            {
                case lbracket:
                    LexicalAnalysis.NextSym(); Expression();
                    while (Symbol == comma)
                    {
                        LexicalAnalysis.NextSym(); Expression();
                    }
                    Accept(rbracket);
                    break;
                case point:
                    LexicalAnalysis.NextSym(); Accept(ident);
                    break;
                case arrow:
                    LexicalAnalysis.NextSym();
                    break;
            }
    }
    void VarDeclaration()
    {
        Accept(varsy);
        while (Symbol == comma)
        {
            LexicalAnalysis.NextSym();
            Accept(ident);
        }
        Accept(colon);
        type();
    }
    void VarPart()
    {
        if (Symbol == varsy)
        {
            Accept(varsy);
            do
            {
                VarDeclaration();
                Accept(semicolon);
            }
            while (Symbol == ident);
        };
    }
    void ArrayType()
    {
        Accept(arraysy);
        Accept(lbracket);
        SimpleType();
        while (Symbol == comma)
        {
            LexicalAnalysis.NextSym();
            SimpleType();
        }
        Accept(rbracket);
        Accept(ofsy);
        type();
    }
    /// <summary>
    /// Accepts current symbol if it is equal to <paramref name="expectedSymbol"/> and moves to next symbol
    /// </summary>
    /// <returns>True if symbol is accepted.</returns>
    bool Accept(byte expectedSymbol)
    {
        if (Symbol == expectedSymbol)
        {
            LexicalAnalysis.NextSym();
            return true;
        }
        else
        {
            InputOutput.LineErrors().Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = LexicalAnalysis.Pos,
                    SpecificErrorDescription = $"Expected {Keywords.InverseKw[expectedSymbol]}"
                }
            );
            return false;
        }
    }

}