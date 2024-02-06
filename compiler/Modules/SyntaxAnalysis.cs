namespace Modules;

using System;
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
    void CompoundStatement()
    {
        Accept(beginsy); 
        Operator();
        while (Symbol == semicolon)
        {
            LexicalAnalysis.NextSym(); 
            Operator();
        }
        Accept(endsy);
    }

    private void Operator()
    {
        throw new NotImplementedException();
    }

    void Block()
    {
        LabelPart();
        ConstPart();
        TypePart();
        VarPart();
        ProcfuncPart();
        OperatorPart();
    }

    private void OperatorPart()
    {
        throw new NotImplementedException();
    }

    void WhileStatement()
    {
        Accept(whilesy);
        Expression();
        Accept(dosy);
        Operator();    
    }
    void ForStatement()
    {
        Accept(forsy);
        Accept(ident);
        Accept(assign);
        Expression();
        if (Symbol == tosy || Symbol == downtosy)
            LexicalAnalysis.NextSym();
        Expression();
        Accept(dosy);
        Operator();
    }
    bool type()
    {
        // return SimpleType() ||  ComposedType() || ReferencedType();
        return false;
    }
    //<простой тип> ::= <перечислимый тип> | <ограниченный тип> | <имя типа>
    void SimpleType()
    {
        if(Symbol=='('){
            EnumType();
            return;
        }
        // if(Constants.Contains(symbol))
    }
    bool TypeName(){
        return Accept(ident);
    }
    bool IndexedType(){
        throw new NotImplementedException();

    }
    bool EnumType(){
        throw new NotImplementedException();

    }
    bool ComposedType(){
        throw new NotImplementedException();
    }
    bool ReferencedType(){
        throw new NotImplementedException();
    }
    void VariableRecord(){
        Variable();
    }
    void VariableFile(){
        Variable();
    }
    void Name(){
        Accept(ident);
    }
    void FieldName(){
        Accept(ident);
    }
    void WholeWithoutSign(){
        Accept(intc);
    }

    private void ProcfuncPart()
    {
        throw new NotImplementedException();
    }

    private void TypePart()
    {
        throw new NotImplementedException();
    }

    private void ConstPart()
    {
        throw new NotImplementedException();
    }

    private void LabelPart()
    {
        throw new NotImplementedException();
    }

    void Expression()
    {
        SimpleExpression();

        if(RelationOperations.Contains(Symbol)){
            RelationOperation();
            SimpleExpression();
        }
    }
    byte[] RelationOperations = [latergreater,greater,later,laterequal,greaterequal,insy];
    private void RelationOperation()
    {
        Accept(RelationOperations);
    }

    byte[] AdditiveOperations = [plus,minus,orsy];
    private void SimpleExpression()
    {
        SignOrEmpty();
        Term();
        while(AdditiveOperations.Contains(Symbol) && !InputOutput.EOF){
            AdditiveOperation();
            Term();
        }
    }

    private void AdditiveOperation()
    {
        Accept(AdditiveOperations);
    }

    private void SignOrEmpty()
    {
        byte[] signs = [plus,minus];
        var sign = signs.FirstOrDefault(s=>s==Symbol,(byte)0);
        if(sign == 0) return;
        Accept(sign);
    }
    byte[] MultiplicativeOperations = [star,divsy,slash,modsy,andsy];
    private void Term()
    {
        Factor();
        while(MultiplicativeOperations.Contains(Symbol)){
            MultiplicativeOperation();
            Factor();
        }
    }
    private void MultiplicativeOperation()
    {
        Accept(MultiplicativeOperations);
    }
    byte[] ConstantWithoutSign = [intc,floatc,stringc,nilsy];
    void Factor(){
        if(Symbol== ident){
            Variable();
            return;
        }
        if(ConstantWithoutSign.Contains(Symbol)){
            Accept(ConstantWithoutSign);
            return;
        }
        if(Symbol==leftpar){
            Accept(leftpar);
            Expression();
            Accept(rightpar);
        }
        //TODO: function declaration
        //TODO: set
        if(Symbol==notsy){
            Accept(notsy);
            Factor();
        }
    }

    void Programme()
    {
        Accept(programsy);
        Accept(ident);
        Accept(semicolon);
        Block();
        Accept(point);
    }

    void Variable()
    {
        Accept(ident);
        while (Symbol == lbracket || Symbol == point || Symbol == arrow)
            switch (Symbol)
            {
                case lbracket:
                    LexicalAnalysis.NextSym(); Expression();
                    while (Symbol == comma)
                    {
                        LexicalAnalysis.NextSym(); Expression();
                    }
                    Accept(rbracket);
                    break;
                case point:
                    LexicalAnalysis.NextSym(); Accept(ident);
                    break;
                case arrow:
                    LexicalAnalysis.NextSym();
                    break;
            }
    }
    void VarDeclaration()
    {
        Accept(varsy);
        while (Symbol == comma)
        {
            LexicalAnalysis.NextSym();
            Accept(ident);
        }
        Accept(colon);
        type();
    }
    void VarPart()
    {
        if (Symbol == varsy)
        {
            Accept(varsy);
            do
            {
                VarDeclaration();
                Accept(semicolon);
            }
            while (Symbol == ident);
        };
    }
    void ArrayType()
    {
        Accept(arraysy);
        Accept(lbracket);
        SimpleType();
        while (Symbol == comma)
        {
            LexicalAnalysis.NextSym();
            SimpleType();
        }
        Accept(rbracket);
        Accept(ofsy);
        type();
    }
    void IfStatement(){
        Accept(ifsy);
        Expression();
        Accept(thensy);
        Operator();
        if(Symbol == elsesy){
            LexicalAnalysis.NextSym();
            Operator();
        }
    }
    bool Accept(byte[] anyOfThisSymbols){
        var op = anyOfThisSymbols.FirstOrDefault(s=>s==Symbol,(byte)0);
        if(op==0){
            var symbols = string.Join(" ",anyOfThisSymbols.Select(s=>Keywords.InverseKw[s]));
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

}