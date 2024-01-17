using System.Collections.Concurrent;
using System.Text;

namespace Modules;
public class LexicalAnalysis
{
    public LexicalAnalysis(InputOutput inputOutput)
    {
        InputOutput = inputOutput;
        Token = new()
        {
            CharNumber = 0,
            LineNumber = 0
        };
    }
    public TextPosition Token;
    public byte Symbol;
    public string SymbolValue="";
    public InputOutput InputOutput { get; }
    bool SkipComments(){
        if(InputOutput.EOF) return false; 
        if(InputOutput.CurrentLine == "") return false;
        var isCommentLine =  InputOutput.Char=='/' && InputOutput.PeekChar=='/';
        if(isCommentLine){
            InputOutput.NextLine();
            return true;
        }
        return false;
    }
    public void NextSym()
    {
        //add constants check

        bool condition() => !InputOutput.EOF && (InputOutput.CurrentLine == "" || InputOutput.Char == ' ' || InputOutput.Char == '\t');
        while (condition()) InputOutput.NextChar();
        if(SkipComments()){
            NextSym();
            return;
        }

        Token.CharNumber = InputOutput.Pos.CharNumber;
        Token.LineNumber = InputOutput.Pos.LineNumber;
        var lineNumber = InputOutput.Pos.LineNumber;

        if (InputOutput.Errors.TryGetValue(lineNumber, out var lineErrors))
        {
        }
        else
        {
            lineErrors = InputOutput.Errors[lineNumber] = new List<Error>();
        }

        //if our symbol is not string
        //accumulate current string characters into string until keywords search stops recognizing
        //input sequence as legal
        var currentLine = InputOutput.CurrentLine;
        var accumulator = new StringBuilder();
        var charNumber = InputOutput.Pos.CharNumber;
        var substring = "";
        long errorCode = -1;
        var nextChar = currentLine[Math.Min(charNumber+1,currentLine.Length-1)];
        //find and handle all allowed symbols
        for (; ; )
        {
            if (char.IsDigit(InputOutput.Char) || (InputOutput.Char=='.' && char.IsDigit(nextChar)))
            {
                //take until Char.IsDigit or .
                var number =
                new string(
                    currentLine
                    .Skip(charNumber)
                    .TakeWhile(c => Char.IsDigit(c) || c == '.')
                    .ToArray()
                );
                substring = number;

                var isFloating = number.Contains('.');

                //search for float constant
                if (isFloating)
                {
                    var sym = Keywords.SearchFloatConstant(number, out errorCode);
                    if (errorCode < 0 && sym is not null)
                    {
                        Symbol = (byte)sym;
                        break;
                    }
                }
                else
                {
                    //if error search for int constant
                    var sym = Keywords.SearchIntConstant(number, out errorCode);
                    if (errorCode < 0 && sym is not null)
                    {
                        Symbol = (byte)sym;
                        break;
                    }
                }
                //else add error
                lineErrors.Add(new Error()
                {
                    ErrorCode = errorCode,
                    Position = InputOutput.Pos
                });
                break;
            }

            if (char.IsLetter(InputOutput.Char))
            {
                // take until isLetter,digits and underscores
                var name =
                new string(
                    currentLine
                    .Skip(charNumber)
                    .TakeWhile(c => Char.IsLetterOrDigit(c) || c == '_')
                    .ToArray()
                );
                substring = name;
                //check if it is keyword
                var sym = Keywords.SearchKeyword(name, out errorCode);
                if (errorCode < 0 && sym is not null)
                {
                    Symbol = (byte)sym;
                    break;
                }

                //check if variable name
                sym = Keywords.SearchVariable(name, out errorCode);
                if (errorCode < 0 && sym is not null)
                {
                    Symbol = (byte)sym;
                    //save variable name
                    break;
                }
                //else add error
                lineErrors.Add(new Error()
                {
                    ErrorCode = errorCode,
                    Position = InputOutput.Pos
                });
                break;
            }

            if (InputOutput.Char == '\'')
            {
                //search for closing \' symbol and check for string constant
                var count = 0;
                var name =
                new string(
                    currentLine
                    .Skip(charNumber)
                    .TakeWhile(c =>{
                        if( c == '\''){
                            count++;
                            if(count==2)
                            return true;
                        }
                        return count<2;
                    })
                    .ToArray()
                );
                substring = name;

                //check if it is string constant
                var sym = Keywords.SearchStringConstant(name, out errorCode);
                if (errorCode < 0 && sym is not null)
                {
                    Symbol = (byte)sym;
                    break;
                }
                //else add error
                lineErrors.Add(new Error()
                {
                    ErrorCode = errorCode,
                    Position = InputOutput.Pos
                });
                break;
            }

            //handle basic operations such as + - / * := = ( [ ) ] etc
            //they're all at most of length 2 so we can just check forward two elements

            for (int i = 2; i > 0; i--)
            {
                substring =
                new string(
                    currentLine
                    .Skip(charNumber)
                    .Take(i)
                    .ToArray()
                );
                var sym = Keywords.SearchKeyword(substring, out errorCode);
                if (errorCode < 0 && sym is not null)
                {
                    Symbol = (byte)sym;
                    break;
                }
            }
            //else add error
            if (errorCode >= 0)
                lineErrors.Add(new Error()
                {
                    ErrorCode = errorCode,
                    Position = InputOutput.Pos
                });
            break;
        }

        //now call NextChar enough times to cover found substring
        if (substring != "")
        {
            foreach (var c in substring)
                InputOutput.NextChar();
        }
        SymbolValue = substring;
        
        // now pray to God that everything works
        // System.Console.WriteLine(substring);
    }
}