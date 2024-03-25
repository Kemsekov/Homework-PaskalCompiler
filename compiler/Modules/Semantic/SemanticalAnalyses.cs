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
    /// <summary>
    /// Список идентификаторов относительно каждого уровня.
    /// Все глобальные переменные/функции/константы итд находятся в Identifiers[0]
    /// Локальные идентификаторы для глобальной функции можно найти в Identifiers[1]
    /// И.т.д вплоть до какого угодня уровня вложенности.
    /// При выходе из области видимости все локальные идентификаторы затераются - 
    /// т.е пока ты обрабатываешь тело глобальной функции все локальные идентификаторы 
    /// сохраняются в Identifiers[1], но как только происходит выход из тела функции 
    /// Identifiers[1] становится пустым.
    /// Т.е текущие значения Identifiers хранят список идентификаторов валидных
    /// только для текущего контекса
    /// </summary>
    IList<IDictionary<string,IdentifierInfo>> Identifiers;

    public SemanticalAnalyses(SyntaxAnalysis source) : base(source.LexicalAnalysis,source.InputOutput,source.ErrorDescriptions,source.Configuration)
    {
        _source=source;
        Identifiers = new List<IDictionary<string, IdentifierInfo>>();
    }
    public override bool AcceptHadError { 
        get => _source.AcceptHadError; 
        set => _source.AcceptHadError=value; 
    }
    /// <summary>
    /// Ищет самый актуальный идентификатор (предпочитая локальные глобальным).
    /// Вернет null если не найден идентификатор с данным именем
    /// </summary>
    IdentifierInfo? SearchIdentifier(string name){
        foreach(var i in Identifiers.Reverse()){
            if(i is null || i.Count==0) continue;
            if(i.TryGetValue(name,out var value))
                return value;
        }
        return null;
    }
    protected override bool Accept(byte expectedSymbol)
    {
        return base.Accept(expectedSymbol);
    }
    
}