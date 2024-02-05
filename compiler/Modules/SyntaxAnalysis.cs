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
    /// <summary>
    /// A set of symbols that is acceptable after key symbol
    /// </summary>
    public IDictionary<byte, HashSet<byte>> StartSymbols;
    void CompoundStatement()
    {
        Accept(beginsy); 
        Statement();
        while (Symbol == semicolon)
        {
            LexicalAnalysis.NextSym(); 
            Statement();
        }
        Accept(endsy);
    }
    void Block()
    {
        LabelPart();
        ConstPart();
        TypePart();
        VarPart();
        ProcfuncPart();
        StatementPart();
    }
    void WhileStatement()
    {
        Accept(whilesy);
        Expression();
        Accept(dosy);
        Statement();
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
        Statement();
    }
    bool type()
    {
        return SimpleType() ||  ComposedType() || ReferencedType();
    }
    //<простой тип> ::= <перечислимый тип> | <ограниченный тип> | <имя типа>
    bool SimpleType()
    {
        return EnumType() || IndexedType() || TypeName();
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
    private void Statement()
    {
        throw new NotImplementedException();
    }

    private void StatementPart()
    {
        throw new NotImplementedException();
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
        var relation = RelationOperations.FirstOrDefault(v=>v==Symbol,(byte)0);
        if(relation==0){
            InputOutput.LineErrors().Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = LexicalAnalysis.Pos,
                    SpecificErrorDescription = $"Expected relation operation"
                }
            );
            return;
        }
        Accept(relation);
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
        var op = AdditiveOperations.FirstOrDefault(s=>s==Symbol,(byte)0);
        if(op==0){
            InputOutput.LineErrors().Add(
                new Error
                {
                    ErrorCode = (long)ErrorCodes.UnexpectedSymbol,
                    Position = LexicalAnalysis.Pos,
                    SpecificErrorDescription = $"Expected additive operation `+ - or`"
                }
            );
            return;
        }
        Accept(op);
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
            // MultiplicativeOperation();
            Factor();
        }
    }
    void Factor(){
        Variable();
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
        Statement();
        if(Symbol == elsesy){
            LexicalAnalysis.NextSym();
            Statement();
        }
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