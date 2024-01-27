namespace Modules;
public class SyntaxAnalysis
{
    public SyntaxAnalysis(LexicalAnalysis lexical, ConfigurationVariables configuration)
    {
        Configuration = configuration;  
        LexicalAnalysis = lexical;
    }
    public TextPosition Pos => LexicalAnalysis.Pos;
    public ConfigurationVariables Configuration { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    void Accept(byte expectedSymbol){
        if(LexicalAnalysis.Symbol==expectedSymbol){
            LexicalAnalysis.NextSym();
        }
        else{
            LexicalAnalysis.InputOutput.LineErrors().Add(
                new Error{
                    ErrorCode= (long)ErrorCodes.UnexpectedSymbol
                }
            );
        }
    }
}