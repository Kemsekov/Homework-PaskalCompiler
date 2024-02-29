using Eto.Parse;
using Modules;

public class MainHostedService : BackgroundService
{
    public MainHostedService(InputOutput inputOutput,LexicalAnalysis lexicalAnalysis,SyntaxAnalysis syntaxAnalysis)
    {
        InputOutput=inputOutput;
        LexicalAnalysis = lexicalAnalysis;
        SyntaxAnalysis=syntaxAnalysis;
    }

    public InputOutput InputOutput { get; }
    public LexicalAnalysis LexicalAnalysis { get; }
    public SyntaxAnalysis SyntaxAnalysis { get; }

    public void PrintProgramErrors()
    {
        EtoParseAttempt();
        return;
        //just read all file and accumulate errors
        // while (!InputOutput.EOF)
        // {
        //     // InputOutput.NextChar();
        //     LexicalAnalysis.NextSym();
        //     System.Console.WriteLine($"{LexicalAnalysis.SymbolValue} \t {LexicalAnalysis.Symbol} \t {Keywords.InverseKw[LexicalAnalysis.Symbol]}");
        // }
        
        //just run block of syntax analysis
        SyntaxAnalysis.StartBlock();
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

    void EtoParseAttempt(){
        var bnf = new Eto.Parse.Grammars.EbnfGrammar(Eto.Parse.Grammars.EbnfStyle.Iso14977);
        // var g = bnf.Build(File.ReadAllText("grammar.bnf"),"start block");
        var a = 
"""
input = name, number | number;
name = {letter};
number = [sign] , digit , {digit};
sign='+'|'-';
""";
        var g = bnf.Build(a,"input");
        var a1 = g.Match("125421");
        var a2 = g.Match("-125421");
        var a3 = g.Match("+125421");
        var a4 = g.Match("Vladislav 91823");
        var a5 = g.Match("Vladislav -91823");
        var a6 = g.Match("Vladislav +91823");

    }
}