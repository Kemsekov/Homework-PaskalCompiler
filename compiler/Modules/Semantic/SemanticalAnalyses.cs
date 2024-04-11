using static Lexical;
using Modules.Semantic;
using Microsoft.VisualBasic;
using Modules.Nodes;
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
public class SemanticalAnalyses : SyntaxTreeFactory
{
    string[] SupportedTypes = ["integer", "float", "char", "string", "byte", "boolean"];
    SyntaxAnalysis _source;
    IdentifierStorage identifierStorage;
    /// <summary>
    /// Вызывается после успешного вызова Accept.
    /// Принимает на вход принятый символ и его значение.
    /// </summary>
    Action<byte, string, TextPosition> AfterAccept = (_, _, _) => { };
    public SemanticalAnalyses(SyntaxAnalysis source) : base(source)
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
    // для каждой пары значений фактор операция фактор
    // сделать анализ того что используются корректные типы
    public override void Term()
    {
        base.Term();
        if(AcceptedNode is not Term tr) return;
        //factor {op factor}
        if(tr.Children.Count()==1){
            tr.Type=(tr.Children.First() as Factor ?? null)?.Type;
            return;
        }
        var tokens = tr.Tokens();
        var f1 =  tr.Children.ElementAt(0) as Factor;
        var op1 = tr.Children.ElementAt(1) as MultiplicativeOperationCall;
        var f2 =  tr.Children.ElementAt(2) as Factor;
        if(f1 is null || op1 is null || f2 is null) return;
        // mul_op   = { * / mod and div }
        // float_op = { * / mod }
        // bool_op  = { and }

        // int   float_op float    = float
        // float float_op int      = float
        // float float_op float    = float
        // boolean bool_op boolean = boolean
        // int div int             = int
        // int mod int             = int
        // int float_op int        = int
        // else error
        var intType = new Semantic.SimpleType{Name="integer"};
        var floatType = new Semantic.SimpleType{Name="float"};
        Semantic.SimpleType? IsNumber(ITypedTerm n){

            if(intType.Equals(n.Type))
                return intType;
            if(floatType.Equals(n.Type))
                return floatType;
            return null;
        }
        var f1Type = IsNumber(f1);
        var f2Type = IsNumber(f2);
        if(f1Type is null){
            var firstPos  = tokens[0].TextPosition;
            InputOutput.LineErrors(firstPos.LineNumber).Add(
                new Error{
                    ErrorCode= (long)ErrorCodes.NotNumericTerm,
                    Position=firstPos,
                    SpecificErrorDescription="first term is not recognized"
                }
            );
        }
        if(f2Type is null){
            var firstPos  = tokens[0].TextPosition;
            InputOutput.LineErrors(firstPos.LineNumber).Add(
                new Error{
                    ErrorCode= (long)ErrorCodes.NotNumericTerm,
                    Position=firstPos,
                    SpecificErrorDescription="second term is not recognized"
                }
            );
        }
        if(f1Type is null || f2Type is null) return;

        

    }
    public override void MultiplicativeOperationCall()
    {
        base.MultiplicativeOperationCall();
    }
    //анализируем из всех подтипов только константы и переменные и (выражения)
    public override void Factor()
    {
        base.Factor();
        if(AcceptedNode is not Factor factor) return;
        factor.Operation(n=>{
            if(factor.Type is not null) return;
            if(n is not ITypedTerm tr) return;
            factor.Type=tr.Type;
        });
    }
    public override void Subexpression()
    {
        base.Subexpression();
        if(AcceptedNode is not Subexpression sube) return;
        IVariableType? variableType = null;
        sube.Operation(n=>{
            if(n is not Expression ex) return;
            variableType=ex.Type;
        });
        sube.Type=variableType;
    }
    //очень упрощенный вариант обработки переменной. Мы ищем только имя переменной iden
    //и по ней определяем тип переменной
    public override void Variable()
    {
        base.Variable();
        if(AcceptedNode is not Variable fv) return;
        var variableName = "";
        var pos = new TextPosition();
        fv.Operation(n=>{
            if(variableName!="") return;
            if(n is Accept acn && acn.Symbol==ident){
                variableName=acn.Value;
                pos=acn.Pos;
            }
        });
        if(variableName=="") return;
        var variableIdentifier = identifierStorage.Search(variableName);
        if(variableIdentifier is null){
            InputOutput.LineErrors(pos.LineNumber).Add(
                new Error{
                    ErrorCode= (long)ErrorCodes.UndefinedVariable,
                    Position=pos,
                    SpecificErrorDescription=$"Unknown variable {variableName}"
                }
            );
            return;
        }
        fv.Type=variableIdentifier.Value.VariableType;
    }
    // упрощенная обработка констант где мы читаем только l-value константы
    // intc floatc stringc
    public override void Constant()
    {
        base.Constant();
        if(AcceptedNode is not Constant c) return;
        var constType = "";
        c.Operation(n=>{
            if(constType!="" || n is not Accept acn) return;
            var sym = acn.Symbol;
            switch(sym){
                case intc:
                    constType="integer";
                break;
                case floatc:
                    constType="float";
                break;
                case stringc:
                    constType="string";
                break;
            }
        });
        if(constType=="") return;
        c.Type=new Semantic.SimpleType(){
            Name=constType
        };
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
        Semantic.SimpleType? simpleTypeHandelr(byte sym, string value, TextPosition pos)
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
            return new Semantic.SimpleType
            {
                Name = value
            };
        }
        ArrayType? arrayTypeHandler(List<(byte sym, string value, TextPosition pos)> acceptedValues)
        {
            var ofsySplit = acceptedValues.Split(v => v.sym == ofsy).ToList();
            var borders = ofsySplit.First().Where(v => v.sym == intc).ToList();

            var rangedTypes = new List<Semantic.RangedType>();
            foreach (var range in borders.Chunk(2))
            {
                var left = range[0];
                var right = range[1];
                var rangeType = new Semantic.RangedType()
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