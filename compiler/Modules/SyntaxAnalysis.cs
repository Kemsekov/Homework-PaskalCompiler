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
    public TextPosition Pos => LexicalAnalysis.Pos;
    public ConfigurationVariables Configuration { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public ErrorDescriptions ErrorDescriptions { get; }
    public InputOutput InputOutput { get; }
    byte Symbol => LexicalAnalysis.Symbol;
#region BasicMethods
    static byte[] CombineSymbols(byte[][] startsSymbols)
    {
        return startsSymbols.SelectMany(s => s).Distinct().ToArray();
    }
    /// <summary>
    /// Do Or operation on a set of methods
    /// </summary>
    /// <param name="m">Start symbols must not intersect. If startSymbols contains zero '0' then it will mean empty symbol is acceptable</param>
    void Or((string name, Action method, byte[] startSymbols)[] m)
    {
        var allowedEmptySymbol = false;
        foreach (var (name,met, start) in m)
        {
            if (start.Contains(Symbol))
            {
                met();
                return;
            }
            allowedEmptySymbol|=start.Contains((byte)0);
        }
        //if none of or statements have worked out, but or statement was allowed then we just return
        if(allowedEmptySymbol) return;
        var symbols = string.Join(" ", m.Select(s => s.name));
        InputOutput.LineErrors().Add(
            new Error
            {
                ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                Position = LexicalAnalysis.Pos,
                SpecificErrorDescription = $"Expected one of '{symbols}'"
            }
        );
    }
    /// <summary>
    /// Repeats method at least minRepeats and until maxRepeats
    /// </summary>
    void Repeat(Action method, byte[] startSymbols, int minRepeats = 0, int maxRepeats = int.MaxValue)
    {
        //call method() min times
        //then while startSymbols.Contains(Symbol) call method() 
        var repeats = 0;
        for (; repeats < minRepeats; repeats++)
            method();
        for (; repeats < maxRepeats; repeats++)
        {
            if (startSymbols.Contains(Symbol))
                method();
            else
                break;
        }
    }
    bool Accept(char[] anyOfThisSymbols)
    {
        return Accept(anyOfThisSymbols.Select(v => (byte)v).ToArray());
    }
    bool Accept(byte[] anyOfThisSymbols)
    {
        var op = anyOfThisSymbols.FirstOrDefault(s => s == Symbol, (byte)0);
        if (op == 0)
        {
            var symbols = string.Join(" ", anyOfThisSymbols.Select(s => Keywords.InverseKw[s]));
            InputOutput.LineErrors().Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = LexicalAnalysis.Pos,
                    SpecificErrorDescription = $"Expected one of operation {symbols}"
                }
            );
            return false;
        }
        return Accept(op);
    }
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
        if (Symbol == expectedSymbol)
        {
            LexicalAnalysis.NextSym();
            return true;
        }
        else
        {
            InputOutput.LineErrors().Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = LexicalAnalysis.Pos,
                    SpecificErrorDescription = $"Expected {Keywords.InverseKw[expectedSymbol]}"
                }
            );
            return false;
        }
    }
#endregion
#region Block&Sections
    public void Block(){
        MarksSection();
        ConstantsSection();
        // TypesSection(); //not in my task
        VariablesSection();
        ProcedureAndFunctionsSection();
        OperatorsSection();
    }
    static byte[] ProcedureAndFunctionsSectionStart = ProcedureOrFunctionDefinitionStart.Append((byte)0).ToArray();
    void ProcedureAndFunctionsSection(){
        Repeat(
            ()=>{ProcedureOrFunctionDefinition();Accept(semicolon);},
            ProcedureOrFunctionDefinitionStart,
            0
        );
    }
    static byte[] OperatorsSectionStart = CompoundOperatorStart;
    void OperatorsSection(){
        CompoundOperator();
    }
    static byte[] ProcedureOrFunctionDefinitionStart = CombineSymbols([FunctionDefinitionStart,/*ProcedureDefinitionStart*/]);
    void ProcedureOrFunctionDefinition(){
        Or([
            // ("procedure definition",ProcedureDefinition,ProcedureDefinitionStart), // not in my task
            ("function definition",FunctionDefinition,FunctionDefinitionStart),
        ]);
    }
    
    static byte[] MarksSectionStart = [labelsy,0];
    void MarksSection(){
        Repeat(
            ()=>{
                Accept(labelsy);
                Mark();
                Repeat(
                    ()=>{
                        Accept(comma);
                        Mark();
                    },
                    [comma],
                    0
                );
            },
            [labelsy],
            0,1
        );
        Accept(semicolon);
    }
    static byte[] ConstantsSectionStart = [constsy,0];
    void ConstantsSection(){
        Repeat(
            ()=>{
                Accept(constsy);
                ConstDefinition();
                Accept(semicolon);
                Repeat(
                    ()=>{
                        ConstDefinition();
                        Accept(semicolon);
                    },
                    ConstDefinitionStart,
                    0
                );
            },
            [constsy],
            0,1
        );
    }
    void VariablesSection(){
        Repeat(
            ()=>{
                Accept(varsy);
                Repeat(()=>{
                        SameTypeVariablesDescription();
                        Accept(semicolon);
                    },
                    SameTypeVariablesDescriptionStart,
                    1
                );
            },
            [varsy],
            0,1
        );
    }
    static byte[] SameTypeVariablesDescriptionStart = [ident];
    void SameTypeVariablesDescription(){
        Accept(ident);
        Repeat(
            ()=>{Accept(comma);Accept(ident);},
            [comma],
            0
        );
        Accept(colon);
        Type_();
    }
    static byte[] TypeStart = []; // TODO:
    void Type_(){
        Or([
            ("simple type",SimpleType,SimpleTypeStart), //I will limit my types to simple ones
            // ("compound type",CompoundType,CompoundTypeStart),
            // ("reference type",ReferenceType,ReferenceTypeStart),
        ]);
    }
    static byte[] ConstDefinitionStart = [ident];
    //ошибка в bnf файле '<определение константы> ::= <имя> = <константа>'
    void ConstDefinition(){
        Accept(ident);
        Accept(equal);
        Constant();
    }
    static byte[] SimpleTypeStart = CombineSymbols([EnumTypeStart,RangedTypeStart,TypeNameStart]);
    void SimpleType(){
        Or([
            ("enum type",EnumType,EnumTypeStart),
            ("ranged type",RangedType,RangedTypeStart),
            ("type name",TypeName,TypeNameStart),
        ]);
    }
    static byte[] EnumTypeStart = [(byte)'('];
    void EnumType(){
        Accept('(');
        Accept(ident);
        Repeat(
            ()=>{Accept(comma);Accept(ident);},
            [comma],
            0
        );
        Accept(')');
    }
    static byte[] RangedTypeStart = ConstantStart;
    void RangedType(){
        Constant();
        Accept(twopoints);
        Constant();
    }
    static byte[] TypeNameStart = [ident];
    void TypeName(){
        Accept(ident);
    }
#endregion
#region Expression
    static byte[] RelationOperation = [equal, latergreater, later, greater, laterequal, greaterequal, insy];
    static byte[] ExpressionStart = SimpleExpressionStart;
    void Expression()
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
    static byte[] SignSymbols = [plus, minus];
    static byte[] AdditiveOperation = [plus, minus, orsy];
    static byte[] SimpleExpressionStart = CombineSymbols([SignSymbols, TermStart]);
    void SimpleExpression()
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
    static byte[] MultiplicativeOperation = [star, slash, divsy, modsy, andsy];
    static byte[] TermStart = FactorStart;
    void Term()
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
    static byte[] ConstantWithoutSign = [intc, floatc, stringc, nilsy];
    static byte[] FactorStart = CombineSymbols([VariableStart, ConstantWithoutSign, [(byte)'('], FunctionDefinitionStart, SetStart, [notsy]]);
    void Factor()
    {
        Or([
            ("variable",Variable,VariableStart),
            ("Constant without sign",()=>Accept(ConstantWithoutSign),ConstantWithoutSign),
            ("(",()=>{Accept('(');Expression();Accept(')');},[(byte)'(']),
            ("function definition",FunctionDefinition,FunctionDefinitionStart),
            ("set",Set,SetStart),
            ("not",()=>{Accept(notsy);Factor();},[notsy]),
        ]);
    }
    static byte[] VariableStart = CombineSymbols([FullVariableStart, VariableComponentStart]);
    void Variable()
    {
        Or([
            ("full variable",FullVariable,FullVariableStart),
            ("variable",VariableComponent,VariableComponentStart),
            // ("specified variable",SpecifiedVariable,SpecifiedVariableStart), //idk what '↑' symbol is
        ]);
    }
    static byte[] FullVariableStart = VariableNameStart;
    void FullVariable()
    {
        VariableName();
    }
    static byte[] VariableNameStart = [ident];
    void VariableName()
    {
        Accept(VariableNameStart);
    }
    static byte[] VariableComponentStart = CombineSymbols([IndexedVariableStart, FieldDefinitionStart]);
    void VariableComponent()
    {
        Or([
            ("indexed variable",IndexedVariable,IndexedVariableStart),
            ("field definition",FieldDefinition,FieldDefinitionStart),
            // ("file buffer",FileBuffer,FileBufferStart), // idk what file buffer '↑' symbol is
        ]);
    }
    static byte[] IndexedVariableStart = VariableArrayStart;
    void IndexedVariable()
    {
        VariableArray();
        Accept((byte)'[');
        Repeat(
            () => { Accept(comma); Expression(); },
            [comma],
            0
        );
        Accept((byte)']');
    }
    static byte[] VariableArrayStart = VariableStart;
    void VariableArray()
    {
        Variable();
    }
    static byte[] FieldDefinitionStart = VariableRecordStart;
    void FieldDefinition()
    {
        VariableRecord();
        Accept(point);
        FieldName();
    }
    static byte[] VariableRecordStart = VariableStart;
    void VariableRecord()
    {
        Variable();
    }
    static byte[] FieldNameStart = [ident];
    void FieldName()
    {
        Accept(ident);
    }
    static byte[] FunctionDefinitionStart = FunctionNameStart;
    void FunctionDefinition()
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
    static byte[] FunctionNameStart = [ident];
    void FunctionName()
    {
        Accept(ident);
    }
    byte[] ActualParameterStart = CombineSymbols([ExpressionStart, VariableStart, ProcedureNameStart, FunctionNameStart]);
    void ActualParameter()
    {
        Or([
            ("expression",Expression,ExpressionStart),
            ("variable",Variable,VariableStart),
            ("procedure name",ProcedureName,ProcedureNameStart),
            ("function name",FunctionName,FunctionNameStart),
        ]);
    }
    static byte[] ProcedureNameStart = [ident];
    void ProcedureName()
    {
        Accept(ident);
    }
    static byte[] SetStart = [(byte)'['];
    void Set()
    {
        Accept('[');
        ElementsList();
        Accept(']');
    }
    //add zero to list so empty symbol is also accepted
    static byte[] ElementsListStart = ElementStart.Append((byte)0).ToArray();
    void ElementsList()
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
    static byte[] ElementStart = ExpressionStart;
    void Element()
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
    static byte[] AssignOperatorStart = CombineSymbols([VariableStart,FunctionNameStart]);
    void AssignOperator(){
        Or([
            ("variable",()=>{
                Variable();
                Accept(equal);
                Expression();
            },VariableStart),
            ("function name",()=>{
                FunctionName();
                Accept(equal);
                Expression();
            },FunctionNameStart)
        ]);
    }
    static byte[] IfOperatorStart = [ifsy];
    void IfOperator(){
        Accept(ifsy);
        Expression();
        Accept(thensy);
        Operator();
        Repeat(
            ()=>{
                Accept(elsesy);
                Operator();
            },
            [elsesy],
            0,1
        );
    }
    static byte[] OperatorStart = CombineSymbols([UnmarkedOperatorStart,MarkStart]);
    void Operator(){
        Or([
            ("unmarked operator",UnmarkedOperator,UnmarkedOperatorStart),
            ("mark",()=>{Mark();UnmarkedOperator();},MarkStart),
        ]);
    }
    static byte[] UnmarkedOperatorStart = CombineSymbols([SimpleOperatorStart,ComplexOperatorStart]);
    void UnmarkedOperator(){
        Or([
            ("simple operator",SimpleOperator,SimpleOperatorStart),
            ("complex operator",ComplexOperator,ComplexOperatorStart),
        ]);
    }
    static byte[] SimpleOperatorStart = 
        CombineSymbols(
            [AssignOperatorStart,ProcedureOperatorStart,GotoOperatorStart])
        .Append((byte)0)
        .ToArray();
    void SimpleOperator(){
        var orPart = ()=>Or([
            (":=",AssignOperator,AssignOperatorStart),
            ("procedure",ProcedureOperator,ProcedureOperatorStart),
            ("goto",GotoOperator,GotoOperatorStart)
        ]);
        Repeat(orPart,SimpleOperatorStart,0,1);
    }
    static byte[] ProcedureOperatorStart = ProcedureNameStart;
    void ProcedureOperator(){
        ProcedureName();
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
    static byte[] GotoOperatorStart = [gotosy];
    void GotoOperator(){
        Accept(gotosy);
        Mark();
    }
    static byte[] MarkStart = [intc];
    void Mark(){
        Accept(intc);
    }
    static byte[] ComplexOperatorStart = CombineSymbols([CompoundOperatorStart,SelectOperatorStart,CycleOperatorStart,AppendOperatorStart]);
    void ComplexOperator(){
        Or([
            ("compound",CompoundOperator,CompoundOperatorStart),
            ("select",SelectOperator,SelectOperatorStart),
            ("cycle",CycleOperator,CycleOperatorStart),
            ("append",AppendOperator,AppendOperatorStart),
        ]);
    }
    static byte[] AppendOperatorStart = [withsy];
    void AppendOperator(){
        Accept(withsy);
        VariableRecordsList();
        Accept(dosy);
        Operator();
    }

    static byte[] VariableRecordsListStart = VariableRecordStart;
    void VariableRecordsList(){
        VariableRecord();
        Repeat(
            ()=>{Accept(comma);VariableRecord();},
            [comma],
            0
        );
    }
    static byte[] CompoundOperatorStart = [beginsy];
    void CompoundOperator(){
        Accept(beginsy);
        Operator();
        Repeat(
            ()=>{Accept(semicolon);Operator();},
            [semicolon],
            0
        );
        Accept(endsy);
    }
    static byte[] SelectOperatorStart = CombineSymbols([IfOperatorStart,VariantOperatorStart]);
    void SelectOperator(){
        Or([
            ("if",IfOperator,IfOperatorStart),
            ("variant",VariantOperator,VariantOperatorStart)
        ]);
    }
    static byte[] VariantOperatorStart = [casesy];
    void VariantOperator(){
        Accept(casesy);
        Expression();
        Accept(ofsy);
        VariantListItem();
        Repeat(
            ()=>{
                Accept(semicolon);
                VariantListItem();
            },
            [semicolon],
            0
        );
        Accept(endsy);
    }
    static byte[] VariantListItemStart = VariantMarksListStart.Append((byte)0).ToArray();
    void VariantListItem(){
        Repeat(
            ()=>{
                VariantMarksList();
                Accept(colon);
                Operator();
            },
            VariantMarksListStart,
            0,1
        );
    }
    static byte[] VariantMarksListStart = VariantMarkStart;
    void VariantMarksList(){
        VariantMark();
        Repeat(
            ()=>{
                Accept(comma);
                VariantMark();
            },
            [comma],
            0
        );
    }
    static byte[] VariantMarkStart = ConstantStart;
    void VariantMark(){
        Constant();
    }
    static byte[] ConstantStart = CombineSymbols([[intc],SignStart,ConstantNameStart,SignStart,[stringc]]);
    void Constant(){
        Or([
            ("unsigned int constant",()=>Accept(intc),[intc]),
            ("int constant",()=>{Sign();Accept(intc);},SignStart),
            ("const name",ConstantName,ConstantNameStart),
            ("const name with sign",()=>{Sign();ConstantName();},SignStart),
            ("string constant",()=>Accept(stringc),[stringc])
        ]);
    }
    static byte[] SignStart = [plus,minus];
    void Sign(){
        Or([
            ("+",()=>Accept(plus),[plus]),
            ("-",()=>Accept(minus),[minus]),
        ]);
    }
    static byte[] ConstantNameStart = [ident];
    void ConstantName(){
        Accept(ident);
    }
    static byte[] CycleOperatorStart = CombineSymbols([WhileCycleStart,ForCycleStart,RepeatCycleStart]);
    void CycleOperator(){
        Or([
            ("while cycle",WhileCycle,WhileCycleStart),
            ("for cycle",ForCycle,ForCycleStart),
            ("do while cycle",RepeatCycle,RepeatCycleStart),
        ]);
    }
#endregion
#region Cycles
    static byte[] WhileCycleStart = [whilesy];
    void WhileCycle(){
        Accept(whilesy);
        Expression();
        Accept(dosy);
        Operator();
    }
    static byte[] ForCycleStart = [forsy];
    void ForCycle(){
        Accept(forsy);
        CycleParameter();
        Accept(equal);
        Expression();
        Direction();
        Expression();
        Accept(dosy);
        Operator();
    }
    static byte[] CycleParameterStart = [ident];
    //in bnf file '<параметр цикла >:= <имя>' is wrong
    void CycleParameter(){
        Accept(ident);
    }
    static byte[] DirectionStart = [tosy,downtosy];
    void Direction(){
        Or([
            ("to",()=>Accept(tosy),[tosy]),
            ("downto",()=>Accept(downtosy),[downtosy]),
        ]);
    }
    static byte[] RepeatCycleStart = [repeatsy];
    void RepeatCycle(){
        Accept(repeatsy);
        Operator();
        Repeat(
            ()=>{
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
}