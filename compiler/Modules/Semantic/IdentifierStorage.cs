using Modules.Semantic;
namespace Modules;

//TODO: почему опять уезжают ошибки дальше места их появления!

/// <summary>
/// Класс который хранит списки идентификаторов
/// </summary>
public class IdentifierStorage
{
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
    IList<IDictionary<string, IdentifierInfo>> Identifiers;
    public IdentifierStorage(){
        Identifiers = new List<IDictionary<string, IdentifierInfo>>();
    }
    /// <summary>
    /// Adds new layer
    /// </summary>
    public void NewLayer(){
        var newInfoMap = new Dictionary<string, IdentifierInfo>();
        Identifiers.Add(newInfoMap);
    }
    /// <summary>
    /// Drops highest layer
    /// </summary>
    public void DropLayer(){
        Identifiers.RemoveAt(Identifiers.Count - 1);
    }
    /// <summary>
    /// Ищет самый актуальный идентификатор (предпочитая локальные глобальным).
    /// Вернет null если не найден идентификатор с данным именем
    /// </summary>
    public IdentifierInfo? Search(string name)
    {
        foreach (var i in Identifiers.Reverse())
        {
            if (i is null || i.Count == 0) continue;
            if (i.TryGetValue(name, out var value))
                return value;
        }
        return null;
    }
    /// <summary>
    /// Removed identifier with given name
    /// </summary>
    public bool Remove(string name)
    {
        foreach (var i in Identifiers.Reverse())
        {
            if (i is null || i.Count == 0) continue;
            if (i.Remove(name))
                return true;
        }
        return false;
    }
    /// <summary>
    /// Adds new identifier
    /// </summary>
    public void Add(IdentifierInfo ind)
    {
        Identifiers.Last().Add(ind.Name, ind);
    }
}
