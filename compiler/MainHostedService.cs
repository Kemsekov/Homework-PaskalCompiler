using Modules;

public class MainHostedService : BackgroundService
{
    public MainHostedService(
        InputOutput inputOutput,
        LexicalAnalysis lexicalAnalysis,
        SyntaxAnalysis syntaxAnalysis, 
        SyntaxTreeFactory syntaxTreeFactory,
        SemanticalAnalyses semantic)
    {
        InputOutput=inputOutput;
        LexicalAnalysis = lexicalAnalysis;
        SyntaxAnalysis=syntaxAnalysis;
        Semantic=semantic;
        SyntaxTreeFactory=syntaxTreeFactory;
    }

    public InputOutput InputOutput { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public SyntaxAnalysis SyntaxAnalysis { get; }
    public SemanticalAnalyses Semantic { get; }
    public SyntaxTreeFactory SyntaxTreeFactory { get; }
    public void PrintProgramErrors()
    {
        //just run block of syntax analysis
        SyntaxTreeFactory.StartBlock();
        System.Console.WriteLine("-------PROGRAM-------");
        //print program and errors
        var end = (ulong)InputOutput.Program.Lines.Length;
        for(ulong i = 0;i<end;i++){
            if(InputOutput.IsErrorOnLine(i))
                InputOutput.PrintErrorsOnLine(i);
            else
                System.Console.WriteLine(InputOutput.Program.Lines[i]);
        }
        System.Console.WriteLine($"\nTotal errors : {InputOutput.ErrorsCounter}");
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PrintProgramErrors();
        return Task.CompletedTask;
    }
}