namespace Modules;


// описание переменных
// описание массивов
// присваивание
// выражения
// условный
// циклы

//декоратор-объект для SyntaxAnalysis
public class SemanticalAnalyses : SyntaxAnalysis
{
    SyntaxAnalysis _source;
    IDictionary<string,IdentifierInfo> Identifiers;

    public SemanticalAnalyses(SyntaxAnalysis source) : base(source.LexicalAnalysis,source.InputOutput,source.ErrorDescriptions,source.Configuration)
    {
        _source=source;
        Identifiers = new Dictionary<string, IdentifierInfo>();
    }
    public override bool AcceptHadError { 
        get => _source.AcceptHadError; 
        set => _source.AcceptHadError=value; 
    }
    protected override bool Accept(byte expectedSymbol)
    {
        return base.Accept(expectedSymbol);
    }
    
}