using static Lexical;
using Modules.Semantic;
using Microsoft.VisualBasic;
using Modules.Nodes;
using System.Linq;
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
    readonly Semantic.SimpleType intType = new Semantic.SimpleType{Name="integer"};
    readonly Semantic.SimpleType floatType = new Semantic.SimpleType{Name="float"};
    readonly Semantic.SimpleType boolType = new Semantic.SimpleType{Name="boolean"};
    readonly Semantic.SimpleType stringType = new Semantic.SimpleType{Name="string"};
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
            if (identifierStorage.Search(v) is IdentifierInfo t){
                t.VariableType = variableType;
                identifierStorage.Remove(v);
                identifierStorage.Add(t);
            }
        }
        // ident - TypeName
        // array[num_const..num_const] of ident
        // array[num_const..num_const] of array
        AfterAccept = (_, _, _) => { };
    }
    #endregion
    #region Выражение
    // мне очень жаль что я решил забить и не создавать код в которой не
    // нарушался бы open/closed принцип, но мне лень переделывать это теперь
    // соболезную всем кто будет читать это
    // мой совет - реализуйте паттер visitor на конструкциях и внутри каждого
    // класса реализуйте логику взаимодействия конструкций между собой
    
    //=, <>, <, >, <=, >=
    //simple_expr {relation simple_expr}
    public override void Expression()
    {
        base.Expression();
        if(AcceptedNode is not Expression e) return;
        
        var haveMissingType = 
        e.Children
        .Select(c=>c as ITypedNodeTerm)
        .Where(t=>t is not null)
        .Any(t=>t?.Type is null);

        if(haveMissingType){
            return;
        }
        if(e.Children.Count()==1){
            e.Type=(e.Children.First() as SimpleExpression)?.Type;
            return;
        }
        var prev = e.Children.First() as SimpleExpression;
        var op = e.Children.ElementAt(1).Tokens()[0];
        var next = e.Children.ElementAt(2) as SimpleExpression;
        var pos = op.TextPosition;

        if(op.Symbol==equal || op.Symbol==latergreater){
            if(!prev?.Type?.Equals(next?.Type) ?? false){
                InputOutput.LineErrors(pos.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                        Position=pos,
                        SpecificErrorDescription=$"cannot apply relation operator to different types"
                    }
                );
            }
        }
        var prevInt = intType.Equals(prev?.Type);
        var prevFloat = floatType.Equals(prev?.Type);
        var prevString = stringType.Equals(prev?.Type);
        var prevbool = stringType.Equals(prev?.Type);
        var prevNum = prevInt || prevFloat;

        var nextInt = intType.Equals(next?.Type);
        var nextFloat = floatType.Equals(next?.Type);
        var nextString = stringType.Equals(next?.Type);
        var nextbool = stringType.Equals(next?.Type);
        var nextNum = nextInt || nextFloat;

        if(prevNum && nextNum){
            e.Type=boolType;
            return;
        }
        InputOutput.LineErrors(pos.LineNumber).Add(
            new Error{
                ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                Position=pos,
                SpecificErrorDescription=$"cannot apply numeric relation operator to non-numeric types"
            }
        );

    }
    //[sign] term {add_op term}
    public override void SimpleExpression()
    {
        base.SimpleExpression();
        if(AcceptedNode is not SimpleExpression simpleExpression) return;
        var children = simpleExpression.Children.ToArray();
        var first = simpleExpression.Children.First();
        if(first is SignSymbolsCall s){
            children=children.Skip(1).ToArray();
            // +|- int
            // +|- float
            // else error
            if(children[0] is not Term t) return;

            if (!intType.Equals(t.Type) && !floatType.Equals(t.Type))
            {
                var pos = t.Tokens()[0].TextPosition;
                InputOutput.LineErrors(pos.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                        Position=pos,
                        SpecificErrorDescription=$"cannot apply operation"
                    }
                );
                return;
            }
        }
        // num_op = +|-
        // bool_op = or
        // int num_op int = int
        // float num_op int = int
        // int num_op float = int
        // bool bool_op bool = bool
        if(children[0] is not Term t1) return;
        if(children.Length==1){
            simpleExpression.Type=t1.Type;
            return;
        }
        byte[] num_op = [plus,minus];
        byte[] bool_op = [orsy];
        var prev = t1;
        foreach(var pair in children.Skip(1).Chunk(2)){
            var op = pair[0] as AdditiveOperationCall;
            if(op is null) continue;
            var pos = op.Tokens()[0].TextPosition;
            var next = pair[1] as Term;
            var opPos = op?.Tokens()[0].TextPosition;
            var opSy = op?.Tokens()[0].Symbol;
            var prevFloat = floatType.Equals(prev?.Type);
            var prevInt = intType.Equals(prev?.Type);
            var prevBool = boolType.Equals(prev?.Type);
            var nextFloat = floatType.Equals(next?.Type);
            var nextInt = intType.Equals(next?.Type);
            var nextBool = boolType.Equals(next?.Type);

            var prevNum = prevFloat || prevInt;
            var nextNum = nextFloat || nextInt;

            if(num_op.Contains(opSy ?? 0)){
                if(prevFloat && nextNum || prevNum && nextFloat){
                    prev = new Term(simpleExpression){
                        Type=floatType
                    };
                    continue;
                }
                if(prevInt && nextInt){
                  prev = new Term(simpleExpression){
                        Type=intType
                    };
                    continue;
                }
                #pragma warning disable
                InputOutput.LineErrors(opPos.Value.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                        Position=opPos.Value,
                        SpecificErrorDescription=$"cannot apply additive operation to given types"
                    }
                );
                prev = null;
                return;
                #pragma warning enable
            }
            if(bool_op.Contains(opSy ?? 0)){
                if(prevBool && nextBool){
                    continue;
                }
                InputOutput.LineErrors(opPos.Value.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                        Position=opPos.Value,
                        SpecificErrorDescription=$"cannot apply or operator on non-bool types"
                    }
                );
                prev = null;
                return;
            }
        }
        simpleExpression.Type=prev.Type;
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
        var prev = tr.Children.First() as Factor;
        foreach(var op_factor in tr.Children.Skip(1).Chunk(2)){
            var op =  op_factor[0] as MultiplicativeOperationCall;
            var next = op_factor[1] as Factor;
            if(prev is null || op is null || next is null) return;
            var type = DetermineType(prev,op,next);
            prev = new Factor(tr)
            {
                Type = type
            };
            
        }
        tr.Type=prev?.Type;

        Semantic.SimpleType? IsNumber(ITypedNodeTerm n){

            if(intType.Equals(n.Type))
                return intType;
            if(floatType.Equals(n.Type))
                return floatType;
            if(boolType.Equals(n.Type))
                return boolType;
            return null;
        }
        // mul_op   = { * / mod and div }
        // float_op = { * / mod }
        // bool_op  = { and }
        // int_op   = { div mod * }

        // int   float_op float    = float
        // float float_op int      = float
        // float float_op float    = float
        // int / int               = float
        // boolean bool_op boolean = boolean
        // int int_op int             = int
        // else error
        Semantic.SimpleType? DetermineType(ITypedNodeTerm f1, Nodes.MultiplicativeOperationCall op_call, ITypedNodeTerm f2){

            var f2Pos = f2.Tokens()[0].TextPosition;

            var f1Type = IsNumber(f1);
            var f2Type = IsNumber(f2);
            var opToken = op_call.Tokens()[0];
            var op = opToken.Symbol;
            if(f1Type is null){
                InputOutput.LineErrors(opToken.TextPosition.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.NotNumericTerm,
                        Position=opToken.TextPosition,
                    }
                );
            }

            if(f2Type is null){
                InputOutput.LineErrors(f2Pos.LineNumber).Add(
                    new Error{
                        ErrorCode= (long)ErrorCodes.NotNumericTerm,
                        Position=f2Pos,
                    }
                );
            }
            if(f1Type is null || f2Type is null) return null;
            byte[] float_op = [star,slash,modsy];
            byte[] int_op = [star,divsy,modsy];
            byte[] bool_op = [andsy];
            byte[] numeric_op = float_op.Concat(int_op).Distinct().ToArray();
            
            //ensure that both f1 and f2 is numeric if numeric operation is used
            if(numeric_op.Contains(op)){
                if(f1Type.Equals(boolType) || f2Type.Equals(boolType)){
                    InputOutput.LineErrors(opToken.TextPosition.LineNumber).Add(
                        new Error{
                            ErrorCode= (long)ErrorCodes.UnsupportedOperation,
                            Position=opToken.TextPosition,
                            SpecificErrorDescription="cannot apply numeric operation on boolean"
                        }
                    );
                    return null;
                }
                if(f1Type.Equals(floatType) || f2Type.Equals(floatType)){
                    return floatType;
                }
            }
            if(op==slash)
                return floatType;
            if(f1Type.Equals(intType) && f2Type.Equals(intType) && int_op.Contains(op))
                return intType;

            if(f1Type.Equals(boolType) && f2Type.Equals(boolType) && bool_op.Contains(op)){
                return boolType;
            }
            InputOutput.LineErrors(opToken.TextPosition.LineNumber).Add(
                        new Error{
                            ErrorCode= (long)ErrorCodes.CannotDeduceTypeFromOperation,
                            Position=opToken.TextPosition,
                        }
                    );
            return null;
        }

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
        IVariableType? t = null;
        factor.Type = (factor.Children.FirstOrDefault(v=>v is ITypedNodeTerm) as ITypedNodeTerm)?.Type;
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
    public override void ConstantWithoutSignCall()
    {
        base.ConstantWithoutSignCall();
        if(AcceptedNode is not ConstantWithoutSignCall c) return;
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