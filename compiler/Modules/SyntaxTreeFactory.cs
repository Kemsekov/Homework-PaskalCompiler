
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
    SyntaxAnalysis _source;
    public SyntaxTreeFactory(SyntaxAnalysis source) : base(source.LexicalAnalysis, source.InputOutput, source.ErrorDescriptions, source.Configuration)
    {
        _source = source;
    }

    public override bool AcceptHadError
    {
        get => _source.AcceptHadError;
        set => _source.AcceptHadError = value;
    }
    protected override bool Accept(byte expectedSymbol)
    {
        return base.Accept(expectedSymbol);
    }
    public override void ActualParameter()
    {
        base.ActualParameter();
    }
    public override void AdditiveOperationCall()
    {
        base.AdditiveOperationCall();
    }
    public override void AppendOperator()
    {
        base.AppendOperator();
    }
    public override void AssignOperator()
    {
        base.AssignOperator();
    }
    public override void CombinedType()
    {
        base.CombinedType();
    }
    public override void ComplexOperator()
    {
        base.ComplexOperator();
    }
    public override void CompoundOperator()
    {
        base.CompoundOperator();
    }
    public override void CompoundType()
    {
        base.CompoundType();
    }
    public override void Constant()
    {
        base.Constant();
    }
    public override void ConstantName()
    {
        base.ConstantName();
    }
    public override void ConstantsSection()
    {
        base.ConstantsSection();
    }
    public override void ConstantWithoutSignCall()
    {
        base.ConstantWithoutSignCall();
    }
    public override void ConstDefinition()
    {
        base.ConstDefinition();
    }
    public override void ConstNameWithSign()
    {
        base.ConstNameWithSign();
    }
    public override void CycleOperator()
    {
        base.CycleOperator();
    }
    public override void CycleParameter()
    {
        base.CycleParameter();
    }
    public override void Direction()
    {
        base.Direction();
    }
    public override void Element()
    {
        base.Element();
    }
    public override void ElementsList()
    {
        base.ElementsList();
    }
    public override void EnumType()
    {
        base.EnumType();
    }
    public override void Expression()
    {
        base.Expression();
    }
    public override void Factor()
    {
        base.Factor();
    }
    public override void FactorNegation()
    {
        base.FactorNegation();
    }
    public override void FieldDefinition()
    {
        base.FieldDefinition();
    }
    public override void FieldName()
    {
        base.FieldName();
    }
    public override void ForCycle()
    {
        base.ForCycle();
    }
    public override void FormalParametersSection()
    {
        base.FormalParametersSection();
    }
    public override void FullVariable()
    {
        base.FullVariable();
    }
    public override void FunctionCall()
    {
        base.FunctionCall();
    }
    public override void FunctionDefinition()
    {
        base.FunctionDefinition();
    }
    public override void FunctionName()
    {
        base.FunctionName();
    }
    public override void GotoOperator()
    {
        base.GotoOperator();
    }
    public override void IfOperator()
    {
        base.IfOperator();
    }
    public override void IndexedVariable()
    {
        base.IndexedVariable();
    }
    public override void IntConstant()
    {
        base.IntConstant();
    }
    public override void Label()
    {
        base.Label();
    }
    public override void LabelsSection()
    {
        base.LabelsSection();
    }
    public override void MultiplicativeOperationCall()
    {
        base.MultiplicativeOperationCall();
    }
    public override void Operator()
    {
        base.Operator();
    }
    public override void OperatorsSection()
    {
        base.OperatorsSection();
    }
    public override void ParametersGroup()
    {
        base.ParametersGroup();
    }
    public override void ProcedureAndFunctionsSection()
    {
        base.ProcedureAndFunctionsSection();
    }
    public override void ProcedureDefinition()
    {
        base.ProcedureDefinition();
    }
    public override void ProcedureName()
    {
        base.ProcedureName();
    }
    public override void ProcedureOperator()
    {
        base.ProcedureOperator();
    }
    public override void ProcedureOrFunctionDefinition()
    {
        base.ProcedureOrFunctionDefinition();
    }
    public override void RangedType()
    {
        base.RangedType();
    }
    public override void RegularType()
    {
        base.RegularType();
    }
    public override void RelationOperationCall()
    {
        base.RelationOperationCall();
    }
    public override void RepeatCycle()
    {
        base.RepeatCycle();
    }
    public override void SameTypeVariablesDescription()
    {
        base.SameTypeVariablesDescription();
    }
    public override void SelectOperator()
    {
        base.SelectOperator();
    }
    public override void Set()
    {
        base.Set();
    }
    public override void SetType()
    {
        base.SetType();
    }
    public override void Sign()
    {
        base.Sign();
    }
    public override void SignSymbolsCall()
    {
        base.SignSymbolsCall();
    }
    public override void SimpleExpression()
    {
        base.SimpleExpression();
    }
    public override void SimpleOperator()
    {
        base.SimpleOperator();
    }
    public override void SimpleType()
    {
        base.SimpleType();
    }
    public override void StartBlock()
    {
        base.StartBlock();
    }
    public override void StringConstant()
    {
        base.StringConstant();
    }
    public override void Subexpression()
    {
        base.Subexpression();
    }
    public override void Term()
    {
        base.Term();
    }
    public override void Type_()
    {
        base.Type_();
    }
    public override void TypeName()
    {
        base.TypeName();
    }
    public override void TypesSection()
    {
        base.TypesSection();
    }
    public override void UnlabeledOperator()
    {
        base.UnlabeledOperator();
    }
    public override void UnpackedCompoundType()
    {
        base.UnpackedCompoundType();
    }
    public override void UnsignedIntConstant()
    {
        base.UnsignedIntConstant();
    }
    public override void Variable()
    {
        base.Variable();
    }
    public override void VariableArray()
    {
        base.VariableArray();
    }
    public override void VariableComponent()
    {
        base.VariableComponent();
    }
    public override void VariableName()
    {
        base.VariableName();
    }
    public override void VariableRecord()
    {
        base.VariableRecord();
    }
    public override void VariableRecordsList()
    {
        base.VariableRecordsList();
    }
    public override void VariablesSection()
    {
        base.VariablesSection();
    }
    public override void VariantLabel()
    {
        base.VariantLabel();
    }
    public override void VariantLabelsList()
    {
        base.VariantLabelsList();
    }
    public override void VariantListItem()
    {
        base.VariantListItem();
    }
    public override void VariantOperator()
    {
        base.VariantOperator();
    }
    public override void WhileCycle()
    {
        base.WhileCycle();
    }
    public override void FunctionAssignOperator()
    {
        base.FunctionAssignOperator();
    }
    public override void FunctionParametersGroup()
    {
        base.FunctionParametersGroup();
    }
    public override void IdentParametersGroup()
    {
        base.IdentParametersGroup();
    }
    public override void LabeledOperator()
    {
        base.LabeledOperator();
    }
    public override void ProcedureParametersGroup()
    {
        base.ProcedureParametersGroup();
    }
    public override void VariableAssignOperator()
    {
        base.VariableAssignOperator();
    }
    public override void VarParametersGroup()
    {
        base.VarParametersGroup();
    }
}