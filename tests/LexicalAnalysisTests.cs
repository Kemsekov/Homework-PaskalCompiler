using Modules;

namespace tests;

public class LexicalAnalysisTests
{
    public static InputOutput GetInputOutput(string programPath)
    {
        var prog = new ProgramText(File.ReadAllLines(programPath));
        var disc = new ErrorDescriptions(Enumerable.Range(0, 100).ToDictionary(i => (long)i, i => i.ToString()));
        var conf = new ConfigurationVariables()
        {
            ERRMAX = 20,
            MAXINT = short.MaxValue,
            MAXLINE = 2048
        };
        var input = new InputOutput(prog, disc, conf);
        return input;
    }
    [Fact]
    public void ApproxSqrt()
    {
        var inputOutput = GetInputOutput("Pascal_programs/approx_sqrt.pas");
        var lexical = new LexicalAnalysis(inputOutput);
        
    }
}