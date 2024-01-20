using System.Collections.Concurrent;
using System.Text;

namespace Modules;
public class LexicalAnalysis
{
    public LexicalAnalysis(InputOutput inputOutput, ConfigurationVariables configuration)
    {
        Configuration = configuration;  
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

    public ConfigurationVariables Configuration { get; }
    public InputOutput InputOutput { get; }
    bool SkipComments(){

        if(InputOutput.EOF) return false; 
        if(InputOutput.CurrentLine == "") return false;

        if(InputOutput.Char=='}'){
            InputOutput.LineErrors().Add(new Error{
                    ErrorCode=10, //missing closing '}'
                    Position=InputOutput.Pos
            });
            InputOutput.NextChar();
            return true;
        }

        if(InputOutput.Char=='*' && InputOutput.PeekChar==')'){
            InputOutput.LineErrors().Add(new Error{
                    ErrorCode=9, //missing closing '}'
                    Position=InputOutput.Pos
            });
            InputOutput.NextChar();
            return true;
        }

        var isCommentLine =  InputOutput.Char=='/' && InputOutput.PeekChar=='/';
        if(isCommentLine){
            InputOutput.NextLine();
            return true;
        }

        var startPos = InputOutput.Pos;
        var line = InputOutput.CurrentLine;
        
        isCommentLine =  InputOutput.Char=='(' && InputOutput.PeekChar=='*';
        if(isCommentLine){
            bool condition()=> !(InputOutput.EOF || InputOutput.Char == '*' && InputOutput.PeekChar == ')');
            while(condition()) InputOutput.NextChar();
            
            if(InputOutput.EOF){
                InputOutput.SwitchPosition(startPos);
                InputOutput.LineErrors().Add(new Error{
                    ErrorCode=9, //wrong comment formatting
                    Position=InputOutput.Pos
                });
                InputOutput.PrintErrorsOnCurrentLine();
                InputOutput.NextLine();
            }
            else{
                InputOutput.NextChar();
                InputOutput.NextChar();
            }
            return true;
        }
        isCommentLine =  InputOutput.Char=='{';
        if(isCommentLine){
            //search for enclosing '}'
            bool condition() => !InputOutput.EOF && InputOutput.Char != '}';
            while(condition()) InputOutput.NextChar();
            
            //if we didn't find enclosing '}' return error
            if(InputOutput.EOF){
                InputOutput.SwitchPosition(startPos);
                InputOutput.LineErrors().Add(new Error{
                    ErrorCode=10, //missing closing '}'
                    Position=InputOutput.Pos
                });
                InputOutput.PrintErrorsOnCurrentLine();
                InputOutput.NextLine();
            }
            else
                InputOutput.NextChar();
            return true;
        }
        return false;
    }
    public void NextSym()
    {
        if(InputOutput.EOF) return;

        bool condition() => !InputOutput.EOF && (InputOutput.CurrentLine == "" || InputOutput.Char == ' ' || InputOutput.Char == '\t');
        while (condition()) InputOutput.NextChar();

        if(InputOutput.Char=='@'){
            var a = 1;
        }

        Token.CharNumber = InputOutput.Pos.CharNumber;
        Token.LineNumber = InputOutput.Pos.LineNumber;
        var lineNumber = InputOutput.Pos.LineNumber;
        
        if(SkipComments()){
            NextSym();
            return;
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
        var lineErrors = InputOutput.LineErrors(lineNumber);
        byte sym = Lexical.undefined;
        //find and handle all allowed symbols
        for (; ; )
        {
            // if (char.IsDigit(InputOutput.Char) || (InputOutput.Char=='.' && char.IsDigit(nextChar)))
            if (char.IsDigit(InputOutput.Char))
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
                    sym = Keywords.SearchFloatConstant(number, out errorCode);
                    if (errorCode < 0 )
                    {
                        break;
                    }
                }
                else
                {
                    //if error search for int constant
                    sym = Keywords.SearchIntConstant(number, out errorCode);
                    if (errorCode < 0 )
                    {
                        //also check for int constant to be smaller than MAXINT
                        long converted = long.Parse(number);
                        if(converted>Configuration.MAXINT){
                            lineErrors.Add(new Error()
                            {
                                ErrorCode = 7,//int overflow
                                Position = InputOutput.Pos
                            });
                        }
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
                sym = Keywords.SearchKeyword(name, out errorCode);
                if (errorCode < 0 )
                {
                    Symbol = (byte)sym;
                    break;
                }

                //check if variable name
                sym = Keywords.SearchVariable(name, out errorCode);
                if (errorCode < 0 )
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
                sym = Keywords.SearchStringConstant(name, out errorCode);
                if (errorCode < 0 )
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
                sym = Keywords.SearchKeyword(substring, out errorCode);
                if (errorCode < 0 )
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
        Symbol=sym;

        // now pray to God that everything works
        // System.Console.WriteLine(substring);
    }
}