using static Lexical;
using Modules.Semantic;
using Microsoft.VisualBasic;
namespace Modules;
//TODO: почему опять уезжают ошибки дальше места их появления!

// описание переменных
// описание массивов
// присваивание
// выражения
// условный
// циклы

//декоратор-объект для SyntaxAnalysis
public class SemanticalAnalyses : SyntaxAnalysis
{
    string[] SupportedTypes = ["integer", "float", "char", "string", "byte"];
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
    IList<IDictionary<string, IdentifierInfo>> Identifiers;
    /// <summary>
    /// Вызывается после успешного вызова Accept.
    /// Принимает на вход принятый символ и его значение.
    /// </summary>
    Action<byte, string, TextPosition> AfterAccept = (_, _, _) => { };
    public SemanticalAnalyses(SyntaxAnalysis source) : base(source.LexicalAnalysis, source.InputOutput, source.ErrorDescriptions, source.Configuration)
    {
        _source = source;
        Identifiers = new List<IDictionary<string, IdentifierInfo>>();
    }
    public override bool AcceptHadError
    {
        get => _source.AcceptHadError;
        set => _source.AcceptHadError = value;
    }
    /// <summary>
    /// Ищет самый актуальный идентификатор (предпочитая локальные глобальным).
    /// Вернет null если не найден идентификатор с данным именем
    /// </summary>
    IdentifierInfo? SearchIdentifier(string name)
    {
        foreach (var i in Identifiers.Reverse())
        {
            if (i is null || i.Count == 0) continue;
            if (i.TryGetValue(name, out var value))
                return value;
        }
        return null;
    }
    bool RemoveIdentifier(string name)
    {
        foreach (var i in Identifiers.Reverse())
        {
            if (i is null || i.Count == 0) continue;
            if (i.Remove(name))
                return true;
        }
        return false;
    }
    void AddIdentifier(IdentifierInfo ind)
    {
        Identifiers.Last().Add(ind.Name, ind);
    }
    protected override bool Accept(byte expectedSymbol)
    {
        var currentSymbol = Symbol;
        var currentSymbolValue = SymbolValue;
        var currentPos = Pos;
        if (base.Accept(expectedSymbol))
        {
            AfterAccept(currentSymbol, currentSymbolValue, currentPos);
            return true;
        }
        return false;
    }
    #region НоваяОбласть
    void NewZone(Action insideWork)
    {
        var newInfoMap = new Dictionary<string, IdentifierInfo>();
        Identifiers.Add(newInfoMap);
        insideWork();
        Identifiers.RemoveAt(Identifiers.Count - 1);
    }
    public override void FunctionDefinition()
    {
        NewZone(base.FunctionDefinition);
    }
    public override void ProcedureDefinition()
    {
        NewZone(base.ProcedureDefinition);
    }
    public override void StartBlock()
    {
        NewZone(base.StartBlock);
    }
    #endregion
    #region ПоискВхождений
    public override void SameTypeVariablesDescription()
    {
        var localVariables = new List<string>();
        //добавляем однотипные переменные
        AfterAccept = (sym, value, pos) =>
        {
            if (sym != ident) return;
            var ind = SearchIdentifier(value);
            //если уже добавлена переменная вызываем ошибку
            if (ind is not null)
            {
                InputOutput.LineErrors(pos.LineNumber).Add(new Error
                {
                    ErrorCode = (long)ErrorCodes.VariableAlreadyDefined,
                    Position = pos,
                    SpecificErrorDescription = $"value '{value}'"
                });
                return;
            }
            localVariables.Add(value);
            //добавляем переменную пока с неизвестным типом
            AddIdentifier(new IdentifierInfo
            {
                IdentifierType = IdentifierType.VARS,
                Name = value
            });
        };

        Accept(ident);
        Repeat(
            () => { Accept(comma); Accept(ident); },
            [comma],
            0
        );
        Accept(colon);
        var variableType = ReadType();
        if (variableType is null) return;
        foreach (var v in localVariables)
        {
            if (SearchIdentifier(v) is IdentifierInfo t)
                t.VariableType = variableType;
        }
        // ident - TypeName
        // array[num_const..num_const] of ident
        // array[num_const..num_const] of array
        AfterAccept = (_, _, _) => { };
    }
    #endregion
    IVariableType? ReadType()
    {
        var acceptedValues = new List<(byte sym, string value, TextPosition pos)>();
        //обработчик простого типа
        AfterAccept = (sym, value, pos) =>
        {
            acceptedValues.Add((sym, value, pos));
        };
        Type_();
        if (AcceptHadError) return null;
        SimpleType? simpleTypeHandelr(byte sym, string value, TextPosition pos)
        {
            if (!SupportedTypes.Contains(value))
            {
                InputOutput.LineErrors(pos.LineNumber).Add(new Error
                {
                    ErrorCode = (long)ErrorCodes.TypeNotSupported,
                    Position = pos,
                    SpecificErrorDescription = $"value '{value}'"
                });
                return null;
            }
            return new SimpleType
            {
                Name = value
            };
        }
        ArrayType? arrayTypeHandler(List<(byte sym, string value, TextPosition pos)> acceptedValues)
        {
            var ofsySplit = acceptedValues.Split(v => v.sym == ofsy).ToList();
            var borders = ofsySplit.First().Where(v => v.sym == intc).ToList();

            var rangedTypes = new List<RangedType>();
            foreach (var range in borders.Chunk(2))
            {
                var left = range[0];
                var right = range[1];
                var rangeType = new RangedType()
                {
                    Lower = int.Parse(left.value),
                    Upper = int.Parse(right.value)
                };
                if (rangeType.Lower < 0)
                {
                    InputOutput.LineErrors(left.pos.LineNumber).Add(new Error()
                    {
                        ErrorCode = (long)ErrorCodes.InvalidArrayDimension,
                        Position = left.pos,
                        SpecificErrorDescription = $"Array left index must be greater or equal to 0"
                    });
                    return null;
                }
                if(rangeType.Upper < rangeType.Lower){
                    InputOutput.LineErrors(right.pos.LineNumber).Add(new Error()
                    {
                        ErrorCode = (long)ErrorCodes.InvalidArrayDimension,
                        Position = right.pos,
                        SpecificErrorDescription = $"Array right index must be greater or equal to left index"
                    });
                    return null;
                }
                rangedTypes.Add(rangeType);
            }
            var arrayBaseType =
                ofsySplit
                .Skip(1)
                .SelectMany(v => v).ToList();
            if (arrayBaseType.Count == 1)
            {
                var t = arrayBaseType[0];
                var baseT = simpleTypeHandelr(t.sym, t.value, t.pos);
                if (baseT is null) return null;
                return new ArrayType
                {
                    Dimensions = rangedTypes.ToArray(),
                    ElementType = baseT
                };
            }
            var elementType = arrayTypeHandler(acceptedValues.SkipWhile(v => v.sym != ofsy).Skip(1).ToList());
            if (elementType is null) return null;
            return new ArrayType
            {
                Dimensions = rangedTypes.ToArray(),
                ElementType = elementType
            };
        }
        if (acceptedValues.Count == 1)
        {
            var f = acceptedValues.First();
            return simpleTypeHandelr(f.sym, f.value, f.pos);
        }
        return arrayTypeHandler(acceptedValues);
    }
    public override void RangedType()
    {
        base.RangedType();
    }
}