using static Lexical;
using Modules.Semantic;
using Microsoft.VisualBasic;
namespace Modules;

// создай синтаксическое дерево для всех конструкций
// добавляй в него элементы перегрузив все нужные методы и перед их base вызовом
// добавляй в дерево имя конструкции.
// после base вызова делай анализ дерева на то чтоб он был правильным.
// всю эту тему с построением дерева сделай как декоратор над синтаксическим анализатором
// и пускай этот класс исключительно выполняет функцию заполнения дерева, а 
// твой семантический пускай от него наследуется.

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
    IdentifierStorage identifierStorage;
    /// <summary>
    /// Вызывается после успешного вызова Accept.
    /// Принимает на вход принятый символ и его значение.
    /// </summary>
    Action<byte, string, TextPosition> AfterAccept = (_, _, _) => { };
    public SemanticalAnalyses(SyntaxAnalysis source) : base(source.LexicalAnalysis, source.InputOutput, source.ErrorDescriptions, source.Configuration)
    {
        _source = source;
        identifierStorage = new();
    }
    public override bool AcceptHadError
    {
        get => _source.AcceptHadError;
        set => _source.AcceptHadError = value;
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
        identifierStorage.NewLayer();
        insideWork();
        identifierStorage.DropLayer();
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
            var ind = identifierStorage.Search(value);
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
            identifierStorage.Add(new IdentifierInfo
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
            if (identifierStorage.Search(v) is IdentifierInfo t)
                t.VariableType = variableType;
        }
        // ident - TypeName
        // array[num_const..num_const] of ident
        // array[num_const..num_const] of array
        AfterAccept = (_, _, _) => { };
    }
    #endregion
    #region Выражение
    public override void Expression()
    {
        base.Expression();
    }
    public override void SimpleExpression()
    {
        base.SimpleExpression();
    }
    public override void RelationOperationCall()
    {
        base.RelationOperationCall();
    }
    public override void SignSymbolsCall()
    {
        base.SignSymbolsCall();
    }
    public override void AdditiveOperationCall()
    {
        base.AdditiveOperationCall();
    }
    public override void Term()
    {
        base.Term();
    }
    public override void MultiplicativeOperationCall()
    {
        base.MultiplicativeOperationCall();
    }
    public override void Factor()
    {
        base.Factor();
    }
    public override void ConstantWithoutSignCall()
    {
        base.ConstantWithoutSignCall();
    }
    public override void Subexpression()
    {
        base.Subexpression();
    }
    #endregion
    #region Переменная
    public override void Variable()
    {
        base.Variable();
    }
    public override void FullVariable()
    {
        base.FullVariable();
    }
    public override void VariableComponent()
    {
        base.VariableComponent();
    }
    public override void IndexedVariable()
    {
        base.IndexedVariable();
    }
    public override void FieldDefinition()
    {
        base.FieldDefinition();
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
                if (rangeType.Upper < rangeType.Lower)
                {
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