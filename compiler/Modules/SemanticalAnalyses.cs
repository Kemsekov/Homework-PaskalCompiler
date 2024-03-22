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
    private SyntaxAnalysis _source;

    public SemanticalAnalyses(SyntaxAnalysis source) : base(source.LexicalAnalysis,source.InputOutput,source.ErrorDescriptions,source.Configuration)
    {
        _source=source;
    }
    public override bool AcceptHadError { get => _source.AcceptHadError; }
}