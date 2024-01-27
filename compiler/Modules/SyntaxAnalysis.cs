namespace Modules;
public class SyntaxAnalysis
{
    public SyntaxAnalysis(LexicalAnalysis lexical,InputOutput inputOutput,ErrorDescriptions errorDescriptions, ConfigurationVariables configuration)
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

    /// <summary>
    /// Accepts current symbol if it is equal to <paramref name="expectedSymbol"/> and moves to next symbol
    /// </summary>
    /// <returns>True if symbol is accepted.</returns>
    bool Accept(byte expectedSymbol){
        if(LexicalAnalysis.Symbol==expectedSymbol){
            LexicalAnalysis.NextSym();
            return true;
        }
        else{
            InputOutput.LineErrors().Add(
                new Error{
                    ErrorCode= (long)ErrorCodes.UnexpectedSymbol,
                    Position=LexicalAnalysis.Pos,
                    SpecificErrorDescription=$"Expected {Keywords.InverseKw[expectedSymbol]}"
                }
            );
            return false;
        }
    }
    
}