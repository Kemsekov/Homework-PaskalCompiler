
using Modules.Nodes;

namespace Modules;

// создай синтаксическое дерево для всех конструкций
// добавляй в него элементы перегрузив все нужные методы и перед их base вызовом
// добавляй в дерево имя конструкции.
// после base вызова делай анализ дерева на то чтоб он был правильным.
// всю эту тему с построением дерева сделай как декоратор над синтаксическим анализатором
// и пускай этот класс исключительно выполняет функцию заполнения дерева, а 
// твой семантический пускай от него наследуется.
//декоратор-объект для SyntaxAnalysis
public class SyntaxTreeFactory : SyntaxAnalysis
{
    /// <summary>
    /// Текущая активная конструкция. 
    /// При появлении вхождения новой конструкции она будет добавлена 
    /// к детям данной вершины.
    /// </summary>
    public INode Head{get;protected set;}
    /// <summary>
    /// Корень синтаксического дерева
    /// </summary>
    public INode Root{get;protected set;}
    /// <summary>
    /// Последняя принятая конструкция
    /// </summary>
    public INode AcceptedNode{get;protected set;}
    SyntaxAnalysis _source;
    public SyntaxTreeFactory(SyntaxAnalysis source) : base(source.LexicalAnalysis, source.InputOutput, source.ErrorDescriptions, source.Configuration)
    {
        _source = source;
        Root = new StartBlock();
        Head = Root;
        AcceptedNode = Root;
    }

    public override bool AcceptHadError
    {
        get => _source.AcceptHadError;
        set => _source.AcceptHadError = value;
    }
    protected override bool Accept(byte expectedSymbol)
    {
        var currentSymbol = Symbol;
        var currentValue = SymbolValue;
        var pos = Pos;
        if(base.Accept(expectedSymbol)){
            Head.AddChild(new Accept(Head,pos,currentValue,currentSymbol));
            return true;
        }
        return false;
    }
    void AddNode(Action construction,INode node){
        var prevHead = Head;
        Head.AddChild(node);
        Head=node;
        construction();
        AcceptedNode = Head;
        if(Head.Children.Count()==0){
            Head.Parent?.RemoveChild(Head);
        }
        if(AcceptHadError){
            Head = Root;
            AcceptedNode=Root;
            return;
        }
        Head=prevHead;
    }
    public override void ActualParameter()
    {
        AddNode(base.ActualParameter,new ActualParameter(Head));
    }
    public override void AdditiveOperationCall()
    {
        AddNode(base.AdditiveOperationCall, new AdditiveOperationCall(Head));
    }
    public override void AppendOperator()
    {
        AddNode(base.AppendOperator, new AppendOperator(Head));
    }
    public override void AssignOperator()
    {
        AddNode(base.AssignOperator, new AssignOperator(Head));
    }
    public override void CombinedType()
    {
        AddNode(base.CombinedType, new CombinedType(Head));
    }
    public override void ComplexOperator()
    {
        AddNode(base.ComplexOperator, new ComplexOperator(Head));
    }
    public override void CompoundOperator()
    {
        AddNode(base.CompoundOperator, new CompoundOperator(Head));
    }
    public override void CompoundType()
    {
        AddNode(base.CompoundType, new CompoundType(Head));
    }
    public override void Constant()
    {
        AddNode(base.Constant, new Constant(Head));
    }
    public override void ConstantName()
    {
        AddNode(base.ConstantName, new ConstantName(Head));
    }
    public override void ConstantsSection()
    {
        AddNode(base.ConstantsSection, new ConstantsSection(Head));
    }
    public override void ConstantWithoutSignCall()
    {
        AddNode(base.ConstantWithoutSignCall, new ConstantWithoutSignCall(Head));
    }
    public override void ConstDefinition()
    {
        AddNode(base.ConstDefinition, new ConstDefinition(Head));
    }
    public override void ConstNameWithSign()
    {
        AddNode(base.ConstNameWithSign, new ConstNameWithSign(Head));
    }
    public override void CycleOperator()
    {
        AddNode(base.CycleOperator, new CycleOperator(Head));
    }
    public override void CycleParameter()
    {
        AddNode(base.CycleParameter, new CycleParameter(Head));
    }
    public override void Direction()
    {
        AddNode(base.Direction, new Direction(Head));
    }
    public override void Element()
    {
        AddNode(base.Element, new Element(Head));
    }
    public override void ElementsList()
    {
        AddNode(base.ElementsList, new ElementsList(Head));
    }
    public override void EnumType()
    {
        AddNode(base.EnumType, new EnumType(Head));
    }
    public override void Expression()
    {
        AddNode(base.Expression, new Expression(Head));
    }
    public override void Factor()
    {
        AddNode(base.Factor, new Factor(Head));
    }
    public override void FactorNegation()
    {
        AddNode(base.FactorNegation, new FactorNegation(Head));
    }
    public override void FieldDefinition()
    {
        AddNode(base.FieldDefinition, new FieldDefinition(Head));
    }
    public override void FieldName()
    {
        AddNode(base.FieldName, new FieldName(Head));
    }
    public override void ForCycle()
    {
        AddNode(base.ForCycle, new ForCycle(Head));
    }
    public override void FormalParametersSection()
    {
        AddNode(base.FormalParametersSection, new FormalParametersSection(Head));
    }
    public override void FullVariable()
    {
        AddNode(base.FullVariable, new FullVariable(Head));
    }
    public override void FunctionCall()
    {
        AddNode(base.FunctionCall, new FunctionCall(Head));
    }
    public override void FunctionDefinition()
    {
        AddNode(base.FunctionDefinition, new FunctionDefinition(Head));
    }
    public override void FunctionName()
    {
        AddNode(base.FunctionName, new FunctionName(Head));
    }
    public override void GotoOperator()
    {
        AddNode(base.GotoOperator, new GotoOperator(Head));
    }
    public override void IfOperator()
    {
        AddNode(base.IfOperator, new IfOperator(Head));
    }
    public override void IndexedVariable()
    {
        AddNode(base.IndexedVariable, new IndexedVariable(Head));
    }
    public override void IntConstant()
    {
        AddNode(base.IntConstant, new IntConstant(Head));
    }
    public override void Label()
    {
        AddNode(base.Label, new Label(Head));
    }
    public override void LabelsSection()
    {
        AddNode(base.LabelsSection, new LabelsSection(Head));
    }
    public override void MultiplicativeOperationCall()
    {
        AddNode(base.MultiplicativeOperationCall, new MultiplicativeOperationCall(Head));
    }
    public override void Operator()
    {
        AddNode(base.Operator, new Operator(Head));
    }
    public override void OperatorsSection()
    {
        AddNode(base.OperatorsSection, new OperatorsSection(Head));
    }
    public override void ParametersGroup()
    {
        AddNode(base.ParametersGroup, new ParametersGroup(Head));
    }
    public override void ProcedureAndFunctionsSection()
    {
        AddNode(base.ProcedureAndFunctionsSection, new ProcedureAndFunctionsSection(Head));
    }
    public override void ProcedureDefinition()
    {
        AddNode(base.ProcedureDefinition, new ProcedureDefinition(Head));
    }
    public override void ProcedureName()
    {
        AddNode(base.ProcedureName, new ProcedureName(Head));
    }
    public override void ProcedureOperator()
    {
        AddNode(base.ProcedureOperator, new ProcedureOperator(Head));
    }
    public override void ProcedureOrFunctionDefinition()
    {
        AddNode(base.ProcedureOrFunctionDefinition, new ProcedureOrFunctionDefinition(Head));
    }
    public override void RangedType()
    {
        AddNode(base.RangedType, new RangedType(Head));
    }
    public override void RegularType()
    {
        AddNode(base.RegularType, new RegularType(Head));
    }
    public override void RelationOperationCall()
    {
        AddNode(base.RelationOperationCall, new RelationOperationCall(Head));
    }
    public override void RepeatCycle()
    {
        AddNode(base.RepeatCycle, new RepeatCycle(Head));
    }
    public override void SameTypeVariablesDescription()
    {
        AddNode(base.SameTypeVariablesDescription, new SameTypeVariablesDescription(Head));
    }
    public override void SelectOperator()
    {
        AddNode(base.SelectOperator, new SelectOperator(Head));
    }
    public override void Set()
    {
        AddNode(base.Set, new Set(Head));
    }
    public override void SetType()
    {
        AddNode(base.SetType, new SetType(Head));
    }
    public override void Sign()
    {
        AddNode(base.Sign, new Sign(Head));
    }
    public override void SignSymbolsCall()
    {
        AddNode(base.SignSymbolsCall, new SignSymbolsCall(Head));
    }
    public override void SimpleExpression()
    {
        AddNode(base.SimpleExpression, new SimpleExpression(Head));
    }
    public override void SimpleOperator()
    {
        AddNode(base.SimpleOperator, new SimpleOperator(Head));
    }
    public override void SimpleType()
    {
        AddNode(base.SimpleType, new SimpleType(Head));
    }
    public override void StringConstant()
    {
        AddNode(base.StringConstant, new StringConstant(Head));
    }
    public override void Subexpression()
    {
        AddNode(base.Subexpression, new Subexpression(Head));
    }
    public override void Term()
    {
        AddNode(base.Term, new Term(Head));
    }
    public override void Type_()
    {
        AddNode(base.Type_, new Type_(Head));
    }
    public override void TypeName()
    {
        AddNode(base.TypeName, new TypeName(Head));
    }
    public override void TypesSection()
    {
        AddNode(base.TypesSection, new TypesSection(Head));
    }
    public override void UnlabeledOperator()
    {
        AddNode(base.UnlabeledOperator, new UnlabeledOperator(Head));
    }
    public override void UnpackedCompoundType()
    {
        AddNode(base.UnpackedCompoundType, new UnpackedCompoundType(Head));
    }
    public override void UnsignedIntConstant()
    {
        AddNode(base.UnsignedIntConstant, new UnsignedIntConstant(Head));
    }
    public override void Variable()
    {
        AddNode(base.Variable, new Variable(Head));
    }
    public override void VariableArray()
    {
        AddNode(base.VariableArray, new VariableArray(Head));
    }
    public override void VariableComponent()
    {
        AddNode(base.VariableComponent, new VariableComponent(Head));
    }
    public override void VariableName()
    {
        AddNode(base.VariableName, new VariableName(Head));
    }
    public override void VariableRecord()
    {
        AddNode(base.VariableRecord, new VariableRecord(Head));
    }
    public override void VariableRecordsList()
    {
        AddNode(base.VariableRecordsList, new VariableRecordsList(Head));
    }
    public override void VariablesSection()
    {
        AddNode(base.VariablesSection, new VariablesSection(Head));
    }
    public override void VariantLabel()
    {
        AddNode(base.VariantLabel, new VariantLabel(Head));
    }
    public override void VariantLabelsList()
    {
        AddNode(base.VariantLabelsList, new VariantLabelsList(Head));
    }
    public override void VariantListItem()
    {
        AddNode(base.VariantListItem, new VariantListItem(Head));
    }
    public override void VariantOperator()
    {
        AddNode(base.VariantOperator, new VariantOperator(Head));
    }
    public override void WhileCycle()
    {
        AddNode(base.WhileCycle, new WhileCycle(Head));
    }
    public override void FunctionAssignOperator()
    {
        AddNode(base.FunctionAssignOperator, new FunctionAssignOperator(Head));
    }
    public override void FunctionParametersGroup()
    {
        AddNode(base.FunctionParametersGroup, new FunctionParametersGroup(Head));
    }
    public override void IdentParametersGroup()
    {
        AddNode(base.IdentParametersGroup, new IdentParametersGroup(Head));
    }
    public override void LabeledOperator()
    {
        AddNode(base.LabeledOperator, new LabeledOperator(Head));
    }
    public override void ProcedureParametersGroup()
    {
        AddNode(base.ProcedureParametersGroup, new ProcedureParametersGroup(Head));
    }
    public override void VariableAssignOperator()
    {
        AddNode(base.VariableAssignOperator, new VariableAssignOperator(Head));
    }
    public override void VarParametersGroup()
    {
        AddNode(base.VarParametersGroup, new VarParametersGroup(Head));
    }
}