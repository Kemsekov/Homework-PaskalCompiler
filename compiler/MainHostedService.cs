using Microsoft.Extensions.Hosting;
using Modules;

public class MainHostedService : BackgroundService
{
    public MainHostedService(InputOutput inputOutput,LexicalAnalysis lexicalAnalysis)
    {
        InputOutput=inputOutput;
        LexicalAnalysis = lexicalAnalysis;
    }

    public InputOutput InputOutput { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public void PrintProgramErrors()
    {
        var prevLine = InputOutput.Pos.LineNumber;
        //just read all file and accumulate errors
        while (!InputOutput.EOF)
        {
            // InputOutput.NextChar();
            LexicalAnalysis.NextSym();
            System.Console.WriteLine($"{LexicalAnalysis.SymbolValue} \t {LexicalAnalysis.Symbol} \t {Keywords.InverseKw[LexicalAnalysis.Symbol]}");
        }

        System.Console.WriteLine("-------PROGRAM-------");
        //print program and errors
        var end = (ulong)InputOutput.Program.Lines.Length;
        for(ulong i = 0;i<end;i++){
            if(InputOutput.IsErrorOnLine(i)){
                InputOutput.PrintErrorsOnLine(i);
            }
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