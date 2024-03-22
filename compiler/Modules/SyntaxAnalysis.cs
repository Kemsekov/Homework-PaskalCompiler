
/*
    АВТОР : Бочкарев Владислав ПГНИУ ФИТ-2-2021

    "Вы входите на опасную территорию, и для того
    чтоб выжить вам нужно узнать набор правил и 
    обзовестись необходимыми инструментами - иначе 
    словно ребенок в лесу - вы заблудитесь и вас не станет,
    как утряняя роса исчезает бесследно, так ваша надежда
    покинет вас, ваши силы иссякнут и ваше бездыханное тело
    останется на съедение диким зверям."

    Метод Accept - принимает ожидаемый символ и при несовпадении
    добавляет ошибку.
    Метод Accept который принимает массив - принимает множество ожидаемых символов - 
    Принимает любой ожидаемый символ.

    AcceptHadError - булеан который обозначает была ли ошибка в последнем вызове Accept.
    Когда AcceptHadError=true вся обработка конструкций останавливается и эта переменная
    должна быть поставлена в false вручную(что происходит в методе StartBlock).

    Метод Or
    Принимает на вход массив из кортежей
    (строка с описанием конструкции,метод конструкции, начальные символы конструкции)
    Этот метод на основе начальных символов выбирает какую конструкцию запустить.
    Этот метод необходим чтоб было возможно реализовать конструкции с разветвлениями по типу
    <тип> ::= <простой тип> | <составной тип> | <ссылочный тип>

    Метод Or принимающий массив из массивов символов
    Этот метод отличается от прошлого Or тем что принимает до двух ожидаемых символов.
    Соответственно этот метод смотрит на текущий символ и символ впереди (PeekSymbol) и
    выбирает какую конструкцию запустить.

    Метод Repeat
    Позволяет написать повторяющиеся конструкции.
    Он так же принимает метод конструкции и набор валидных символов, с которых конструкция начинается.
    Так же он принимает минимальное кол-во повторений данной конструкции и максимальное.
    Этим методом можно реализовать "необязательные" конструкции по типу
    <раздел типов> ::= <пусто> | type <определение типа> ;{ <определение типа>;}
    Тут "{ <определение типа>;}" можно реализовать через Repeat с параметром мин кол-ва = 0
    А весь раздел можно реализовать тоже через Repeat с мин кол-вом = 0 и макс кол-вом = 1
    
    
*/
#pragma warning disable
namespace Modules;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Lexical;
public class SyntaxAnalysis
{
    public SyntaxAnalysis(LexicalAnalysis lexical, InputOutput inputOutput, ErrorDescriptions errorDescriptions, ConfigurationVariables configuration)
    {
        Configuration = configuration;
        LexicalAnalysis = lexical;
        ErrorDescriptions = errorDescriptions;
        InputOutput = inputOutput;
    }
    /// <summary>
    /// Sets to true when any Accept encounters error. When it is true no further analysis can be done.
    /// </summary>
    public virtual bool AcceptHadError{get;private set;} = false;
    /// <summary>
    /// Symbol position
    /// </summary>
    public TextPosition Pos => LexicalAnalysis.Pos;
    /// <summary>
    /// Previous symbol position
    /// </summary>
    public TextPosition PrevPos => LexicalAnalysis.PrevPos;
    /// <summary>
    /// Current symbol
    /// </summary>
    byte Symbol => LexicalAnalysis.Symbol;
    public ConfigurationVariables Configuration { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public ErrorDescriptions ErrorDescriptions { get; }
    public InputOutput InputOutput { get; }
    #region BasicMethods
    /// <summary>
    /// Combines many starter symbols into one array
    /// </summary>
    static byte[] CombineSymbols(byte[][] startsSymbols)
    {
        return startsSymbols.SelectMany(s => s).Distinct().ToArray();
    }
    /// <summary>
    /// Combines many starter symbols into one array of expected two starting symbols
    /// </summary>
    static byte[][] CombineSymbols(byte[][][] startsSymbols)
    {
        var res = new byte[2][];
        var firstLayer =  CombineSymbols(startsSymbols.Select(t=>t[0]).ToArray());
        var secondLayer = CombineSymbols(startsSymbols.Where(t=>t.Length>1).Select(t=>t[1]).ToArray());
        return [firstLayer,secondLayer];
    }
    /// <summary>
    /// Do Or operation on a set of methods. 
    /// </summary>
    /// <param name="m">Start symbols must not intersect. If startSymbols contains zero '0' then it will mean empty symbol is acceptable</param>
    public virtual void Or((string name, Action method, byte[] startSymbols)[] m)
    {
        if (AcceptHadError) return;
        var startSymbol = Symbol;
        var startSymbolValue = LexicalAnalysis.SymbolValue;
        var startPos = LexicalAnalysis.Pos;

        var allowedEmptySymbol = false;

        var allowed = m.Select(v => v.startSymbols.Contains(Symbol)).ToArray();

        for (int i = 0; i < m.Length; i++)
        {
            var (name, met, start) = m[i];
            if (allowed[i])
            {
                met();
                return;
            }
            allowedEmptySymbol |= start.Contains((byte)0);
        }
        //if none of or statements have worked out, but or statement was allowed then we just return
        if (allowedEmptySymbol) return;
        AcceptHadError=true;
        var symbols = string.Join(", ", m.Select(s => s.name));
        InputOutput.LineErrors(PrevPos.LineNumber).Add(
            new Error
            {
                ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                Position = PrevPos,
                SpecificErrorDescription = $"Expected one of '{symbols}'"
            }
        );
    }
    /// <summary>
    /// Do Or operation on a set of methods with two expected symbols in forward.
    /// </summary>
    /// <param name="m">Start symbols must not intersect. If startSymbols contains zero '0' then it will mean empty symbol is acceptable</param>
    public virtual void Or((string name, Action method, byte[][] startSymbols)[] m)
    {
        if (AcceptHadError) return;
        var startSymbol = Symbol;
        var startSymbolValue = LexicalAnalysis.SymbolValue;
        var startPos = LexicalAnalysis.Pos;

        var allowedEmptySymbol = false;

        var Peek = LexicalAnalysis.PeekSymbol();
        var allowed =
            m.Select(
                v =>
                v.startSymbols[0].Contains(Symbol) &&
                (v.startSymbols.Length > 1 ? v.startSymbols[1].Contains(Peek ?? 255) : true)
            ).ToArray();

        for (int i = 0; i < m.Length; i++)
        {
            var (name, met, start) = m[i];
            if (allowed[i])
            {
                met();
                return;
            }
            allowedEmptySymbol |= start[0].Contains((byte)0);
        }
        //if none of or statements have worked out, but or statement was allowed then we just return
        if (allowedEmptySymbol) return;
        AcceptHadError=true;
        var symbols = string.Join(", ", m.Select(s => s.name));
        InputOutput.LineErrors(PrevPos.LineNumber).Add(
            new Error
            {
                ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                Position = PrevPos,
                SpecificErrorDescription = $"Expected one of '{symbols}'"
            }
        );
    }
    /// <summary>
    /// Repeats method at least minRepeats and until maxRepeats, until error reached or no start symbols is detected
    /// </summary>
    public virtual void Repeat(Action method, byte[] startSymbols, int minRepeats = 0, int maxRepeats = int.MaxValue)
    {
        if (AcceptHadError) return;
        //call method() min times
        //then while startSymbols.Contains(Symbol) call method() 
        var repeats = 0;
        for (; repeats < minRepeats; repeats++)
            method();

        //if error happened on required iterations 
        //then we just stop
        if (AcceptHadError) return;

        // if error happened on additional operations
        // then just restore old position and return
        var startSymbol = Symbol;
        var startSymbolValue = LexicalAnalysis.SymbolValue;
        var startPos = LexicalAnalysis.Pos;

        for (; repeats < maxRepeats; repeats++)
        {
            if (startSymbols.Contains(Symbol) && !AcceptHadError)
            {
                method();
                if (AcceptHadError) break;
                //save new valid position
                startSymbol = Symbol;
                startSymbolValue = LexicalAnalysis.SymbolValue;
                startPos = LexicalAnalysis.Pos;
            }
            else
                break;
        }
    }
    /// <summary>
    /// Accepts any of given characters, adds error if Symbol is not in given input array
    /// </summary>
    bool Accept(char[] anyOfThisSymbols)
    {
        return Accept(anyOfThisSymbols.Select(v => (byte)v).ToArray());
    }
    /// <summary>
    /// Accepts any of given symbols, adds error if Symbol is not in given input array
    /// </summary>
    bool Accept(byte[] anyOfThisSymbols)
    {
        if (AcceptHadError) return false;
        var op = anyOfThisSymbols.FirstOrDefault(s => s == Symbol, (byte)0);
        if (op == 0)
        {
            if (InputOutput.HaveErrorsAfter(Pos)) return false;

            var symbols = string.Join(" ", anyOfThisSymbols.Select(s => Keywords.InverseKw[s]));
            InputOutput.LineErrors(PrevPos.LineNumber).Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = PrevPos,
                    SpecificErrorDescription = $"Expected one of operation {symbols}"
                }
            );
            AcceptHadError = true;
            return false;
        }
        return Accept(op);
    }
    /// <summary>
    /// Accepts char as symbol
    /// </summary>
    bool Accept(char expectedSymbol)
    {
        return Accept((byte)expectedSymbol);
    }
    /// <summary>
    /// Accepts current symbol if it is equal to <paramref name="expectedSymbol"/> and moves to next symbol
    /// </summary>
    /// <returns>True if symbol is accepted.</returns>
    bool Accept(byte expectedSymbol)
    {
        if (AcceptHadError) return false;
        if (Symbol == expectedSymbol)
        {
            LexicalAnalysis.NextSym();
            return true;
        }
        else
        {
            if (InputOutput.HaveErrorsAfter(Pos)) return false;
            var kw = Keywords.InverseKw[expectedSymbol];

            InputOutput.LineErrors(PrevPos.LineNumber).Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = PrevPos,
                    SpecificErrorDescription = $"Expected {kw}"
                }
            );
            AcceptHadError = true;
            return false;
        }
    }
    #endregion
    #region Block&Sections
    static byte[] BlockStart;
    /// <summary>
    /// Starting point of program syntax analysis.
    /// Start block of syntax analysis. 
    /// It handles errors. 
    /// </summary>
    public void StartBlock()
    {

        Action[] sections = [LabelsSection, ConstantsSection,TypesSection,VariablesSection, ProcedureAndFunctionsSection, OperatorsSection];
        while (!InputOutput.EOF)
        {
            if (AcceptHadError)
                LexicalAnalysis.NextSym();
            while (!InputOutput.EOF && !BlockStart.Contains(Symbol))
            {
                LexicalAnalysis.NextSym();
            }

            AcceptHadError = false;
            foreach (var m in sections)
            {
                m();
                if (AcceptHadError) break;
            }
        }
    }
    public void Block()
    {
        LabelsSection();
        ConstantsSection();
        TypesSection();
        VariablesSection();
        ProcedureAndFunctionsSection();
        OperatorsSection();
    }
    static byte[] TypesSectionStart = [];
    public virtual void TypesSection(){
        //TODO: 
    }
    static byte[] ProcedureAndFunctionsSectionStart;
    public virtual void ProcedureAndFunctionsSection()
    {
        Repeat(
            () => { ProcedureOrFunctionDefinition(); Accept(semicolon); },
            ProcedureOrFunctionDefinitionStart,
            0
        );
    }
    static byte[] OperatorsSectionStart;
    public virtual void OperatorsSection()
    {
        CompoundOperator();
    }
    static byte[] ProcedureOrFunctionDefinitionStart;
    public virtual void ProcedureOrFunctionDefinition()
    {
        Or([
            ("procedure definition",ProcedureDefinition,ProcedureDefinitionStart), // not in my task
            ("function definition",FunctionDefinition,FunctionDefinitionStart),
        ]);
    }

    static byte[] LabelsSectionStart;
    public virtual void LabelsSection()
    {
        Repeat(
            () =>
            {
                Accept(labelsy);
                Label();
                Repeat(
                    () =>
                    {
                        Accept(comma);
                        Label();
                    },
                    [comma],
                    0
                );
                Accept(semicolon);
            },
            [labelsy],
            0, 1
        );
    }
    static byte[] ConstantsSectionStart;
    public virtual void ConstantsSection()
    {
        Repeat(
            () =>
            {
                Accept(constsy);
                ConstDefinition();
                Accept(semicolon);
                Repeat(
                    () =>
                    {
                        ConstDefinition();
                        Accept(semicolon);
                    },
                    ConstDefinitionStart,
                    0
                );
            },
            [constsy],
            0, 1
        );
    }
    static byte[] VariablesSectionStart;
    public virtual void VariablesSection()
    {
        Repeat(
            () =>
            {
                Accept(varsy);
                Repeat(() =>
                {
                    SameTypeVariablesDescription();
                    Accept(semicolon);
                },
                    SameTypeVariablesDescriptionStart,
                    1
                );
            },
            [varsy],
            0, 1
        );
    }
    static byte[] SameTypeVariablesDescriptionStart;
    public virtual void SameTypeVariablesDescription()
    {
        Accept(ident);
        Repeat(
            () => { Accept(comma); Accept(ident); },
            [comma],
            0
        );
        Accept(colon);
        Type_();
    }
    static byte[] TypeStart;
    public virtual void Type_()
    {
        Or([
            ("simple type",SimpleType,SimpleTypeStart), //I will limit my types to simple ones
            ("compound type",CompoundType,CompoundTypeStart),
            // ("reference type",ReferenceType,ReferenceTypeStart),
        ]);
    }
    static byte[] ConstDefinitionStart;
    //ошибка в bnf файле '<определение константы> ::= <имя> = <константа>'
    public virtual void ConstDefinition()
    {
        Accept(ident);
        Accept(equal);
        Constant();
    }
    static byte[] CompoundTypeStart;
    public virtual void CompoundType(){
        Or([
            ("unpacked compound type",UnpackedCompoundType,UnpackedCompoundTypeStart),
            ("packed compound type",()=>{Accept(packedsy);UnpackedCompoundType();},[packedsy]),
        ]);
    }
    static byte[] UnpackedCompoundTypeStart;
    public virtual void UnpackedCompoundType(){
        Or([
            ("regular type",RegularType,RegularTypeStart),
            // ("combined type",CombinedType,CombinedTypeStart),
            // ("set type",SetType,SetTypeStart),
            // ("file type",FileType,FileTypeStart),
        ]);
    }
    static byte[] RegularTypeStart;
    public virtual void RegularType(){
        Accept(arraysy);
        Accept('[');
        SimpleType();
        Repeat(()=>{Accept(comma);SimpleType();},[comma],0);
        Accept(']');
        Accept(ofsy);
        Type_();
    }
    static byte[] CombinedTypeStart = [];
    public virtual void CombinedType(){
        //TODO: 
    }
    static byte[] SetTypeStart = [];
    public virtual void SetType(){
        //TODO: 
    }
    static byte[] SimpleTypeStart;
    public virtual void SimpleType()
    {
        Or([
            ("enum type",EnumType,EnumTypeStart),
            ("type name or ranged type",TypeNameOrRangedType,TypeNameOrRangedTypeStart)
        ]);
    }
    static byte[] EnumTypeStart;
    public virtual void EnumType()
    {
        Accept('(');
        Accept(ident);
        Repeat(
            () => { Accept(comma); Accept(ident); },
            [comma],
            0
        );
        Accept(')');
    }
    static byte[] TypeNameOrRangedTypeStart;
    public virtual void TypeNameOrRangedType()
    {
        Constant();
        Repeat(
            () => { Accept(twopoints); Constant(); },
            [twopoints],
            0, 1
        );
    }
    static byte[] TypeNameStart;
    public virtual void TypeName()
    {
        Accept(ident);
    }
    #endregion
    #region Expression
    static byte[] RelationOperation;
    static byte[] ExpressionStart;
    public virtual void Expression()
    {
        SimpleExpression();
        Repeat(
            () =>
            {
                Accept(RelationOperation);
                SimpleExpression();
            },
            RelationOperation,
            0, 1
        );
    }
    static byte[] SignSymbols;
    static byte[] AdditiveOperation;
    static byte[] SimpleExpressionStart;
    public virtual void SimpleExpression()
    {
        Repeat(
            () =>
            {
                Accept(SignSymbols);
            },
            SignSymbols,
            0, 1
        );
        Term();
        Repeat(
            () =>
            {
                Accept(AdditiveOperation);
                Term();
            },
            AdditiveOperation,
            0
        );
    }
    static byte[] MultiplicativeOperation;
    static byte[] TermStart;
    public virtual void Term()
    {
        Factor();
        Repeat(
            () =>
            {
                Accept(MultiplicativeOperation);
                Factor();
            },
            MultiplicativeOperation,
            0
        );
    }
    static byte[] ConstantWithoutSign;
    static byte[] FactorStart;
    public virtual void Factor()
    {
        Or([
            ("function call",FunctionCall,FunctionCallStart),
            ("const without sign",()=>Accept(ConstantWithoutSign),[ConstantWithoutSign]),
            ("variable",Variable,[VariableStart[0]]),
            ("(",()=>{Accept('(');Expression();Accept(')');},[[(byte)'(']]),
            ("set",Set,[SetStart]),
            ("not",()=>{Accept(notsy);Factor();},[[notsy]]),
        ]);
    }
    static byte[][] VariableStart;
    public virtual void Variable()
    {
        Or([
            ("variable component",VariableComponent,VariableComponentStart),
            ("full variable",FullVariable,[FullVariableStart]),
            // ("specified variable",SpecifiedVariable,SpecifiedVariableStart), //idk what '↑' symbol is
        ]);
    }
    static byte[] FullVariableStart;
    public virtual void FullVariable()
    {
        VariableName();
    }
    static byte[] VariableNameStart;
    public virtual void VariableName()
    {
        Accept(VariableNameStart);
    }
    static byte[][] VariableComponentStart;
    public virtual void VariableComponent()
    {
        Or([
            ("indexed variable",IndexedVariable,IndexedVariableStart),
            ("field definition",FieldDefinition,FieldDefinitionStart),
            // ("file buffer",FileBuffer,FileBufferStart), // idk what file buffer '↑' symbol is
        ]);
    }
    static byte[][] IndexedVariableStart;
    public virtual void IndexedVariable()
    {
        // VariableArray(); //TODO: idk how to fix this recursion
        Accept(ident);
        Accept((byte)'[');
        Expression();
        Repeat(
            () => { Accept(comma); Expression(); },
            [comma],
            0
        );
        Accept((byte)']');
    }
    static byte[] VariableArrayStart;
    public virtual void VariableArray()
    {
        Variable();
    }
    static byte[][] FieldDefinitionStart;
    public virtual void FieldDefinition()
    {
        // VariableRecord();  //TODO: idk how to fix this recursion
        Accept(ident);
        Accept(point);
        FieldName();
    }
    static byte[] VariableRecordStart;
    public virtual void VariableRecord()
    {
        Variable();
    }
    static byte[] FieldNameStart;
    public virtual void FieldName()
    {
        Accept(ident);
    }
    static byte[] FunctionDefinitionStart;
    public virtual void FunctionDefinition()
    {
        Accept(functionsy);
        FunctionName();
        Repeat(
            () =>
            {
                Accept('(');
                FormalParametersSection();
                Repeat(
                    () => { Accept(semicolon); FormalParametersSection(); },
                    [semicolon],
                    0
                );
                Accept(')');
            },
            [(byte)'('],
            0, 1
        );
        Accept(colon);
        TypeName();
        Accept(semicolon);
        Block();
    }
    static byte[][] FunctionCallStart;
    public virtual void FunctionCall()
    {
        FunctionName();
        Repeat(
            () =>
            {
                Accept('(');
                ActualParameter();
                Repeat(
                    () => { Accept(comma); ActualParameter(); },
                    [comma],
                    0
                );
                Accept(')');
            },
            [(byte)'('],
            0, 1
        );
    }
    static byte[] ProcedureDefinitionStart;
    public virtual void ProcedureDefinition(){
        Accept(proceduresy);
        Accept(ident);
        Repeat(
            ()=>{
                Accept('(');
                Repeat(()=>{
                FormalParametersSection();
                Repeat(
                    ()=>{Accept(semicolon);FormalParametersSection();},
                    [semicolon],0
                );
                },FormalParametersSectionStart,0,1);

                Accept(')');
            },
            [(byte)'('],
            0,1
        );
        Accept(semicolon);
        Block();
    }
    static byte[] FormalParametersSectionStart;
    public virtual void FormalParametersSection(){
        Or([
            ("var",()=>{Accept(varsy);ParametersGroup();},[varsy]),
            ("function",()=>{Accept(functionsy);ParametersGroup();},[functionsy]),
            ("procedure",()=>{Accept(proceduresy);ParametersGroup();},[proceduresy]),
            ("ident",ParametersGroup,ParametersGroupStart),
        ]);
    }
    static byte[] ParametersGroupStart;
    public virtual void ParametersGroup(){
        Accept(ident);
        Repeat(
            ()=>{Accept(comma);Accept(ident);},
            [comma],
            0
        );
        Accept(colon);
        Accept(ident);
    }
    static byte[] FunctionNameStart;
    public virtual void FunctionName()
    {
        Accept(ident);
    }
    static byte[] ActualParameterStart;
    public virtual void ActualParameter()
    {
        Or([
            ("variable",Variable,VariableStart),
            ("expression",Expression,[ExpressionStart]),
            ("procedure name",ProcedureName,[ProcedureNameStart]),
            ("function name",FunctionName,[FunctionNameStart]),
        ]);
    }
    static byte[] ProcedureNameStart;
    public virtual void ProcedureName()
    {
        Accept(ident);
    }
    static byte[] SetStart;
    public virtual void Set()
    {
        Accept('[');
        ElementsList();
        Accept(']');
    }
    //add zero to list so empty symbol is also accepted
    static byte[] ElementsListStart;
    public virtual void ElementsList()
    {
        Repeat(
            () =>
            {
                Element();
                Repeat(
                    () => { Accept(comma); Element(); },
                    [comma],
                    0
                );
            },
            ElementStart,
            0, 1
        );
    }
    static byte[] ElementStart;
    public virtual void Element()
    {
        Expression();
        Repeat(
            () =>
            {
                Accept(twopoints);
                Expression();
            },
            [twopoints],
            0, 1
        );
    }
    #endregion
    #region Operators
    static byte[][] AssignOperatorStart;
    public virtual void AssignOperator()
    {

        Or([
            ("variable",()=>{
                Variable();
                Accept(assign);
                Expression();
            },VariableStart[0]),
            ("function name",()=>{
                FunctionName();
                Accept(assign);
                Expression();
            },FunctionNameStart)
        ]);
    }
    static byte[] IfOperatorStart;
    public virtual void IfOperator()
    {
        Accept(ifsy);
        Expression();
        Accept(thensy);
        Operator();
        Repeat(
            () =>
            {
                Accept(elsesy);
                Operator();
            },
            [elsesy],
            0, 1
        );
    }
    static byte[] OperatorStart;
    public virtual void Operator()
    {
        Or([
            ("unlabeled operator",UnlabeledOperator,UnlabeledOperatorStart),
            ("label",()=>{Label();UnlabeledOperator();},LabelStart),
        ]);
    }
    static byte[] UnlabeledOperatorStart;
    public virtual void UnlabeledOperator()
    {
        Or([
            ("simple operator",SimpleOperator,SimpleOperatorStart),
            ("complex operator",ComplexOperator,ComplexOperatorStart),
        ]);
    }
    static byte[] SimpleOperatorStart;
    public virtual void SimpleOperator()
    {
        if(LexicalAnalysis.SymbolValue=="pivot"){
            var a = 1;
        }

        (string,Action,byte[][])[] OrTerms = [
            // (":=",AssignOperator,AssignOperatorStart),
            ("procedure",ProcedureOperator,[ProcedureOperatorStart]),
            ("goto",GotoOperator,[GotoOperatorStart])
        ];

        //this sucks. 
        //AssignOperatorStart and ProcedureOperatorStart is just the same
        //there is not way to distinguish them.
        // //so I use this heuristic here
        var isAssigment = InputOutput.CurrentLine.Contains(":=");

        if(isAssigment){
            OrTerms = OrTerms.Prepend((":=",AssignOperator,AssignOperatorStart)).ToArray();
        }
        var orPart = () => Or(OrTerms);
        Repeat(orPart, SimpleOperatorStart, 0, 1);
    }
    static byte[] ProcedureOperatorStart;
    public virtual void ProcedureOperator()
    {
        ProcedureName();
        Repeat(
            () =>
            {
                Accept('(');
                Repeat(()=>{
                        ActualParameter();
                        Repeat(
                            () => { Accept(comma); ActualParameter(); },
                            [comma],
                            0
                        );
                    },
                    ActualParameterStart,
                    0,1
                );
                Accept(')');
            },
            [(byte)'('],
            0,1
        );

    }
    static byte[] GotoOperatorStart;
    public virtual void GotoOperator()
    {
        Accept(gotosy);
        Label();
    }
    static byte[] LabelStart;
    public virtual void Label()
    {
        Accept(intc);
    }
    static byte[] ComplexOperatorStart;
    public virtual void ComplexOperator()
    {
        Or([
            ("compound",CompoundOperator,CompoundOperatorStart),
            ("select",SelectOperator,SelectOperatorStart),
            ("cycle",CycleOperator,CycleOperatorStart),
            ("append",AppendOperator,AppendOperatorStart),
        ]);
    }
    static byte[] AppendOperatorStart;
    public virtual void AppendOperator()
    {
        Accept(withsy);
        VariableRecordsList();
        Accept(dosy);
        Operator();
    }

    static byte[] VariableRecordsListStart;
    public virtual void VariableRecordsList()
    {
        VariableRecord();
        Repeat(
            () => { Accept(comma); VariableRecord(); },
            [comma],
            0
        );
    }
    static byte[] CompoundOperatorStart;
    public virtual void CompoundOperator()
    {
        Accept(beginsy);
        Operator();
        Repeat(
            () =>
            {
                Accept(semicolon);
                Repeat(Operator, OperatorStart, 0, 1);
            },
            [semicolon],
            0
        );
        Accept(endsy);
    }
    static byte[] SelectOperatorStart;
    public virtual void SelectOperator()
    {
        Or([
            ("if",IfOperator,IfOperatorStart),
            ("variant",VariantOperator,VariantOperatorStart)
        ]);
    }
    static byte[] VariantOperatorStart;
    public virtual void VariantOperator()
    {
        Accept(casesy);
        Expression();
        Accept(ofsy);
        VariantListItem();
        Repeat(
            () =>
            {
                Accept(semicolon);
                VariantListItem();
            },
            [semicolon],
            0
        );
        Accept(endsy);
    }
    static byte[] VariantListItemStart;
    public virtual void VariantListItem()
    {
        Repeat(
            () =>
            {
                VariantLabelsList();
                Accept(colon);
                Operator();
            },
            VariantLabelsListStart,
            0, 1
        );
    }
    static byte[] VariantLabelsListStart;
    public virtual void VariantLabelsList()
    {
        VariantLabel();
        Repeat(
            () =>
            {
                Accept(comma);
                VariantLabel();
            },
            [comma],
            0
        );
    }
    static byte[] VariantLabelStart;
    public virtual void VariantLabel()
    {
        Constant();
    }
    static byte[] ConstantStart;
    public virtual void Constant()
    {
        Or([
            ("unsigned int constant",()=>Accept(intc),[intc]),
            ("int constant",()=>{Sign();Accept(intc);},SignStart),
            ("const name",ConstantName,ConstantNameStart),
            ("const name with sign",()=>{Sign();ConstantName();},SignStart),
            ("string constant",()=>Accept(stringc),[stringc])
        ]);
    }
    static byte[] SignStart;
    public virtual void Sign()
    {
        Or([
            ("+",()=>Accept(plus),[plus]),
            ("-",()=>Accept(minus),[minus]),
        ]);
    }
    static byte[] ConstantNameStart;
    public virtual void ConstantName()
    {
        Accept(ident);
    }
    static byte[] CycleOperatorStart;
    public virtual void CycleOperator()
    {
        Or([
            ("while cycle",WhileCycle,WhileCycleStart),
            ("for cycle",ForCycle,ForCycleStart),
            ("repeated cycle",RepeatCycle,RepeatCycleStart),
        ]);
    }
    #endregion
    #region Cycles
    static byte[] WhileCycleStart;
    public virtual void WhileCycle()
    {
        Accept(whilesy);
        Expression();
        Accept(dosy);
        Operator();
    }
    static byte[] ForCycleStart;
    public virtual void ForCycle()
    {
        Accept(forsy);
        CycleParameter();
        Accept(assign);
        Expression();
        Direction();
        Expression();
        Accept(dosy);
        Operator();
    }
    static byte[] CycleParameterStart;
    //in bnf file '<параметр цикла >:= <имя>' is wrong
    public virtual void CycleParameter()
    {
        Accept(ident);
    }
    static byte[] DirectionStart;
    public virtual void Direction()
    {
        Or([
            ("to",()=>Accept(tosy),[tosy]),
            ("downto",()=>Accept(downtosy),[downtosy]),
        ]);
    }
    static byte[] RepeatCycleStart;
    public virtual void RepeatCycle()
    {
        Accept(repeatsy);
        Operator();
        Repeat(
            () =>
            {
                Accept(semicolon);
                Operator();
            },
            [semicolon],
            0
        );
        Accept(untilsy);
        Expression();
    }
    #endregion
    #region StartSymbolsInitialization
    static SyntaxAnalysis()
    {
        RepeatCycleStart = [repeatsy];
        DirectionStart = [tosy, downtosy];
        CycleParameterStart = [ident];
        ForCycleStart = [forsy];
        WhileCycleStart = [whilesy];
        ConstantNameStart = [ident];
        SignStart = [plus, minus];
        VariantOperatorStart = [casesy];
        CompoundOperatorStart = [beginsy];
        AppendOperatorStart = [withsy];
        LabelStart = [intc];
        GotoOperatorStart = [gotosy];
        IfOperatorStart = [ifsy];
        ProcedureNameStart = [ident];
        SetStart = [(byte)'['];
        FunctionNameStart = [ident];
        FieldNameStart = [ident];
        VariableNameStart = [ident];
        ConstantWithoutSign = [intc, floatc, stringc, nilsy];
        MultiplicativeOperation = [star, slash, divsy, modsy, andsy];
        AdditiveOperation = [plus, minus, orsy];
        SignSymbols = [plus, minus];
        RelationOperation = [equal, latergreater, later, greater, laterequal, greaterequal, insy];
        TypeNameStart = [ident];
        EnumTypeStart = [(byte)'('];
        ConstDefinitionStart = [ident];
        SameTypeVariablesDescriptionStart = [ident];
        VariablesSectionStart = [0, varsy];
        ConstantsSectionStart = [constsy, 0];
        LabelsSectionStart = [labelsy, 0];
        VariableRecordStart = [ident];
        VariableArrayStart = [ident];
        RegularTypeStart=[arraysy];
        ParametersGroupStart=[ident];
        ProcedureDefinitionStart=[proceduresy];
        FunctionDefinitionStart = [functionsy];

        TypesSectionStart=[];
        CombinedTypeStart=[];
        SetTypeStart=[];

        CycleOperatorStart = CombineSymbols([WhileCycleStart, ForCycleStart, RepeatCycleStart]);
        ConstantStart = CombineSymbols([[intc], SignStart, ConstantNameStart, SignStart, [stringc]]);
        VariantLabelStart = ConstantStart;
        VariantLabelsListStart = VariantLabelStart;
        VariantListItemStart = VariantLabelsListStart.Append((byte)0).ToArray();
        SelectOperatorStart = CombineSymbols([IfOperatorStart, VariantOperatorStart]);
        FullVariableStart = VariableNameStart;
        ComplexOperatorStart = CombineSymbols([CompoundOperatorStart, SelectOperatorStart, CycleOperatorStart, AppendOperatorStart]);
        ProcedureOperatorStart = ProcedureNameStart;
        TypeNameOrRangedTypeStart = ConstantStart;
        ProcedureOrFunctionDefinitionStart = CombineSymbols([FunctionDefinitionStart,ProcedureDefinitionStart]);
        ProcedureAndFunctionsSectionStart = ProcedureOrFunctionDefinitionStart.Append((byte)0).ToArray();
        OperatorsSectionStart = CompoundOperatorStart;
        SimpleTypeStart = CombineSymbols([EnumTypeStart, TypeNameOrRangedTypeStart, TypeNameStart]);
        TypeStart = CombineSymbols([SimpleTypeStart,/*CompoundTypeStart,ReferenceTypeStart*/]);
        FieldDefinitionStart = [VariableRecordStart,[point]];
        IndexedVariableStart = [VariableArrayStart,[(byte)'[']];
        VariableComponentStart = CombineSymbols([IndexedVariableStart, FieldDefinitionStart]);
        VariableStart = CombineSymbols([[FullVariableStart], VariableComponentStart]);
        VariableRecordsListStart = VariableRecordStart;
        AssignOperatorStart = [CombineSymbols([VariableStart[0], FunctionNameStart])];
        SimpleOperatorStart =
            CombineSymbols([AssignOperatorStart[0], ProcedureOperatorStart, GotoOperatorStart])
            .Append((byte)0)
            .ToArray();
        UnlabeledOperatorStart = CombineSymbols([SimpleOperatorStart, ComplexOperatorStart]);
        OperatorStart = CombineSymbols([UnlabeledOperatorStart, LabelStart]);
        FactorStart = CombineSymbols([VariableStart[0], ConstantWithoutSign, [(byte)'('], FunctionDefinitionStart, SetStart, [notsy]]);
        TermStart = FactorStart;
        SimpleExpressionStart = CombineSymbols([SignSymbols, TermStart]);
        ExpressionStart = SimpleExpressionStart;
        ElementStart = ExpressionStart;
        ElementsListStart = ElementStart.Append((byte)0).ToArray();
        ActualParameterStart = CombineSymbols([ExpressionStart, VariableStart[0], ProcedureNameStart, FunctionNameStart]);
        BlockStart =
            CombineSymbols([LabelsSectionStart, ConstantsSectionStart, VariablesSectionStart,TypesSectionStart, ProcedureAndFunctionsSectionStart, OperatorsSectionStart])
            .Where(s => s != 0).ToArray();
        UnpackedCompoundTypeStart=CombineSymbols([RegularTypeStart,SetTypeStart]);
        CompoundTypeStart=CombineSymbols([UnpackedCompoundTypeStart,[packedsy]]);
        FormalParametersSectionStart=CombineSymbols([[varsy,proceduresy,functionsy],ParametersGroupStart]);
        FunctionCallStart = [FunctionNameStart,[(byte)'(']];
    }
    #endregion
}