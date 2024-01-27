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
    
}