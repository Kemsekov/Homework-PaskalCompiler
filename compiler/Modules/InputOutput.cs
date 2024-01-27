using System.Collections.Concurrent;

namespace Modules;
public class InputOutput
{
    public InputOutput(ProgramText program, ErrorDescriptions errorDescriptions, ConfigurationVariables configuration)
    {
        Program = program;
        ErrorDescriptions = errorDescriptions;
        Variables = configuration;
        Pos = new TextPosition()
        {
            CharNumber = 0,
            LineNumber = 0
        };
        CurrentLine = Program.Lines[0];
        Errors = new ConcurrentDictionary<ulong, IList<Error>>();
    }
    /// <summary>
    /// Errors on lines of codes.
    /// </summary>
    public IDictionary<ulong, IList<Error>> Errors;
    public ProgramText Program { get; }
    public ErrorDescriptions ErrorDescriptions { get; }
    public ConfigurationVariables Variables { get; }
    /// <summary>
    /// Position of current char in file
    /// </summary>
    public TextPosition Pos;
    /// <summary>
    /// Current line on position <see cref="Pos"/> 
    /// </summary>
    public string CurrentLine { get; protected set; }
    /// <summary>
    /// Current pos char
    /// </summary>
    public char Char => CurrentLine[Pos.CharNumber];
    /// <summary>
    /// Peek one char forward, or null if current char is last in the line
    /// </summary>
    public char? PeekChar => Pos.CharNumber + 1 < CurrentLine.Length ? CurrentLine[Pos.CharNumber + 1] : null;
    /// <summary>
    /// End of file flag
    /// </summary>
    public bool EOF { get; private set; } = false;
    public ulong ErrorsCounter { get; private set; } = 0;

    public IList<Error> LineErrors(ulong lineNumber)
    {
        if (Errors.TryGetValue(lineNumber, out var lineErrors))
        {
        }
        else
        {
            lineErrors = Errors[lineNumber] = new List<Error>();
        }
        return lineErrors;
    }
    public IList<Error> LineErrors()
    {
        return LineErrors(Pos.LineNumber);
    }
    /// <summary>
    /// Skips current line and switches to next line. Use if for example if you need to skip comments for example
    /// </summary>
    public void NextLine()
    {
        if (Pos.LineNumber < (ulong)Program.Lines.Length - 1)
        {
            Pos.LineNumber++;
            Pos.CharNumber = 0;
            CurrentLine = Program.Lines[Pos.LineNumber];
        }
        else
        {
            EOF = true;
        }
    }
    public int PrintErrorsOnCurrentLine()
    {
        if (ErrorsCounter < Variables.ERRMAX && IsErrorOnLine())
        {
            return ListErrors();
        }
        return 0;
    }
    public void NextChar()
    {
        if (Pos.CharNumber >= CurrentLine.Length - 1)
        {
            PrintErrorsOnCurrentLine();
            CurrentLine = "";
            while (string.IsNullOrEmpty(CurrentLine) || string.IsNullOrWhiteSpace(CurrentLine))
            {
                NextLine();
                if (EOF) break;
            }
        }
        else
            Pos.CharNumber++;
    }
    public void SwitchPosition(TextPosition pos){
        Pos = pos;
        CurrentLine = Program.Lines[pos.LineNumber];
        var lastLine = Program.Lines[^1];
        if(pos.LineNumber!=(ulong)(Program.Lines.Length-1) && pos.CharNumber!=lastLine.Length-1){
            EOF=false;
        }
    }
    /// <returns>
    /// True if under current pos line some error is found
    /// </returns>
    bool IsErrorOnLine()
    {
        if (Errors.TryGetValue(Pos.LineNumber, out var errors))
        {
            if (errors.Count != 0)
                return true;
        }
        return false;
    }
    int ListErrors()
    {
        var printedErrors=0;
        //handle line errors
        var errors = Errors[Pos.LineNumber];
        foreach (var e in errors)
        {
            if (ErrorsCounter >= Variables.ERRMAX) break;

            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error on line {Pos.LineNumber}");
            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine(CurrentLine);
            Console.ForegroundColor = ConsoleColor.Red;

            var head = new char[e.Position.CharNumber];
            Array.Fill(head, '-');
            var headStr = new string(head);

            System.Console.WriteLine($"{headStr}^\n{ErrorDescriptions[e.ErrorCode]}\n");

            Console.ResetColor();
            ErrorsCounter++;
            printedErrors++;
        }
        return printedErrors;
    }

}