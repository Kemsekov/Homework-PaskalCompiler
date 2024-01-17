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
    public char? PeekChar =>Pos.CharNumber+1<CurrentLine.Length ? CurrentLine[Pos.CharNumber+1] : null;
    /// <summary>
    /// End of file flag
    /// </summary>
    public bool EOF { get; private set; } = false;
    public ulong ErrorsCounter { get; private set; } = 0;

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
            return;
        }
    }

    public void NextChar()
    {
        if (Pos.CharNumber >= CurrentLine.Length - 1)
        {
            if (ErrorsCounter < Variables.ERRMAX && IsErrorOnLine())
            {
                ListErrors();
            }

            CurrentLine = "";
            while (string.IsNullOrEmpty(CurrentLine) || string.IsNullOrWhiteSpace(CurrentLine)){
                NextLine();
                if(EOF) break;
            }
        }
        else
            Pos.CharNumber++;
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
    void ListErrors()
    {
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
        }
    }

}