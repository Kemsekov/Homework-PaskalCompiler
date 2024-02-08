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
    /// <summary>
    /// True when call of <see cref="NextChar"/> moves cursor to new line 
    /// </summary>
    public bool IsNewLine { get; private set; } = false;
    /// <summary>
    /// Total errors counter
    /// </summary>
    public ulong ErrorsCounter { get; private set; } = 0;

    /// <summary>
    /// A list of errors on a specified line
    /// </summary>
    public IList<Error> LineErrors(ulong lineNumber)
    {
        if (Errors.TryGetValue(lineNumber, out var lineErrors))
        {
        }
        else
        {
            lineErrors = Errors[lineNumber] = new List<Error>();
        }

        //remove repeating errors
        lineErrors = lineErrors.DistinctBy(e => e.Position.CharNumber).ToList();
        Errors[lineNumber] = lineErrors;

        return lineErrors;
    }
    /// <summary>
    /// A list of errors on a current line
    /// </summary>
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
    /// <summary>
    /// Prints errors under given line
    /// </summary>
    /// <param name="line">Sets to current line if not specified</param>
    public int PrintErrorsOnLine(ulong line = ulong.MaxValue)
    {
        line = line == ulong.MaxValue ? Pos.LineNumber : line;
        if (ErrorsCounter < Variables.ERRMAX && IsErrorOnLine(line))
        {
            return ListErrors(line);
        }
        return 0;
    }
    public void NextChar()
    {
        IsNewLine = false;
        if (Pos.CharNumber >= CurrentLine.Length - 1)
        {
            IsNewLine = true;
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
    public void SwitchPosition(TextPosition pos)
    {

        Pos = pos;
        CurrentLine = Program.Lines[pos.LineNumber];
        var lastLine = Program.Lines[^1];
        if (pos.LineNumber != (ulong)(Program.Lines.Length - 1) && pos.CharNumber != lastLine.Length - 1)
        {
            EOF = false;
        }
    }

    public void ClearErrorsAfter(TextPosition pos)
    {
        var currentErrors = LineErrors(pos.LineNumber);
        var errorsAfterCharNumber =
            currentErrors.Where(e => e.Position.CharNumber >= pos.CharNumber)
            .ToList();
        foreach (var e in errorsAfterCharNumber)
            currentErrors.Remove(e);

        //remove all errors in lines after pos
        for (ulong i = pos.LineNumber + 1; i < (ulong)Program.Lines.Length; i++)
        {
            if (Errors.ContainsKey(i))
                Errors[i].Clear();
        }
    }
    public bool HaveErrorsAfter(TextPosition pos)
    {
        var currentErrors = LineErrors(pos.LineNumber);
        var errorsAfterCharNumber =
            currentErrors.Where(e => e.Position.CharNumber >= pos.CharNumber);
        if(errorsAfterCharNumber.Any()) return true;
        for (ulong i = pos.LineNumber + 1; i < (ulong)Program.Lines.Length; i++)
        {
            if (Errors.ContainsKey(i))
            if (Errors[i].Any()) return true;
        }
        return false;
    }

    /// <returns>
    /// True if under given line some error is found
    /// </returns>
    /// <param name="line">Line to check. Uses current line if not specified</param>
    /// <returns></returns>
    public bool IsErrorOnLine(ulong line = ulong.MaxValue)
    {
        line = line == ulong.MaxValue ? Pos.LineNumber : line;
        if (Errors.TryGetValue(line, out var errors))
        {
            if (errors.Count != 0)
                return true;
        }
        return false;
    }
    int ListErrors(ulong line)
    {
        var printedErrors = 0;
        //handle line errors
        var errors = Errors[line];
        var CurrentLine = this.Program.Lines[line];
        if (errors.Count == 0) return 0;

        foreach (var e in errors)
        {
            if (ErrorsCounter >= Variables.ERRMAX) break;

            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error on line {line}");
            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine(CurrentLine);
            Console.ForegroundColor = ConsoleColor.Red;

            var head = new char[e.Position.CharNumber];
            Array.Fill(head, '-');
            var headStr = new string(head);

            System.Console.WriteLine($"{headStr}^\n{ErrorDescriptions[e.ErrorCode]}");
            if (e.SpecificErrorDescription is not null)
                System.Console.WriteLine(e.SpecificErrorDescription);

            System.Console.WriteLine();
            Console.ResetColor();
            ErrorsCounter++;
            printedErrors++;
        }
        return printedErrors;
    }

}