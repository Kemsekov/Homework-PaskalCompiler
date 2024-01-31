using Modules;

namespace tests;

public class TermTests
{
    [Fact]
    public void Arithmetic(){

        //<digit>::= char.IsDigit
        //<letter>::= char.IsLetter
        //<number>::= <digit>{<digit>}
        //<name>::= <letter>{<letter>}
        //<symbol>::= <name>|<number>:|<name>_<number>
        //<sign>={-}
        //<addition>::= +|-
        //<mult> ::= *|/|^|%

        //<simple expression> ::= <sign><symbol>{<op><symbol>}
        //<expression> ::= <simple expression> | (<simple expression>)
        var letter = new Term("<letter>",s=>char.IsLetter(s[0]) ? s[0..1] : throw new Exception("not a letter"));
        var digit = new Term("<digit>",s=>char.IsDigit(s[0]) ? s[0..1] : throw new Exception("not a digit"));

        //<name> ::= <letter>{<letter>}
        var name = letter.Follows(letter.ZeroOrMany()).WithName("<name>");
        name.Validate("vladBochkarev 123");
        Assert.Equal("vladBochkarev",name.LastValidatedPart);
        Assert.Throws<Exception>(()=>name.Validate("12345"));

        //<number> ::= <digit>{<digit>}
        var number = digit.Follows(digit.ZeroOrMany()).WithName("<number>");
        number.Validate("432535 name1");
        Assert.Equal("432535",number.LastValidatedPart);
        Assert.Throws<Exception>(()=>number.Validate("abcde"));

        //<symbol> ::= <name> | <number> | <name>_<number>
        var symbol = name.Or(number,name.Follows("_").Follows(number)).WithName("<symbol>");
        symbol.Validate("123");
        symbol.Validate("name");
        symbol.Validate("supername_123");

        symbol.Validate("543name");
        Assert.Equal("543",symbol.LastValidatedPart);
        symbol.Validate("name123");
        Assert.Equal("name",symbol.LastValidatedPart);
        
        symbol.Validate("name_123_name");
        Assert.Equal("name_123",symbol.LastValidatedPart);
        symbol.Validate("6542_name_123");
        Assert.Equal("6542",symbol.LastValidatedPart);

    }
    [Fact]
    public void OfSelf(){
        var letter = new Term("<letter>",s=>char.IsLetter(s[0]) ? s[0..1] : throw new Exception("not a letter"));
        var digit = new Term("<digit>",s=>char.IsDigit(s[0]) ? s[0..1] : throw new Exception("not a digit"));

        //<number> ::= <digit>{<digit>}
        var number = digit.Follows(digit.ZeroOrMany()).WithName("<number>");
        number.Validate("432535 name1");
        Assert.Equal("432535",number.LastValidatedPart);
        Assert.Throws<Exception>(()=>number.Validate("abcde"));

        var op = Term.OfMany("<op>",["+","-"]);

        //it is a bit tricky not to stuck into infinite recursion
        // <expression> ::= <number><op><number> | (<expression>)
        var expression = 
            number
            .Follows(op)
            .Follows(number)
            .OfSelf(t=>
                Term.OfConstant("(")
                .Follows(t)
                .Follows(Term.OfConstant(")"))
                .Or(t))
            .WithName("<expression>");
        
        expression.Validate("123+86");
        Assert.Equal("123+86",expression.LastValidatedPart);

        expression.Validate("94-388");
        Assert.Equal("94-388",expression.LastValidatedPart);

        expression.Validate("(119-53)");
        Assert.Equal("(119-53)",expression.LastValidatedPart);

        expression.Validate("(((119-53)))");
        Assert.Equal("(((119-53)))",expression.LastValidatedPart);

        //must throws on uneven amount of brackets
        Assert.Throws<Exception>(()=>expression.Validate("((1+43)"));
    }
    [Fact]
    public void OfMany(){
        var name = new Term("<name>",v=>{
            var identLength = v.TakeWhile(c=>char.IsLetter(c) || c=='_').Count();
            if(identLength==0){
                throw new Exception("ident must have name that can contain letters and underscore _");
            }
            return v[0..identLength];
        });

        var action = 
            Term.OfMany("<does>",["likes","dislikes","wants"]);
        var statement = name.Follows(action).Follows(name);

        statement.Validate("vlad likes potatoes");
        Assert.Equal("vlad likes potatoes",statement.LastValidatedPart);

        statement.Validate("dima dislikes carrots");
        Assert.Equal("dima dislikes carrots",statement.LastValidatedPart);
        
        statement.Validate("arina wants marshmallow");
        Assert.Equal("arina wants marshmallow",statement.LastValidatedPart);

        Assert.Throws<Exception>(()=>statement.Validate("vlad hates grechka"));
        Assert.Throws<Exception>(()=>statement.Validate("arina sent message"));
    }
    [Fact]
    public void Or(){
        var name = new Term("<name>",v=>{
            var identLength = v.TakeWhile(c=>char.IsLetter(c) || c=='_').Count();
            if(identLength==0){
                throw new Exception("ident must have name that can contain letters and underscore _");
            }
            return v[0..identLength];
        });

        var action = 
            Term.OfConstant("likes")
            .Or(Term.OfConstant("wants"))
            .Or(Term.OfConstant("dislikes"));
        var statement = name.Follows(action).Follows(name);

        statement.Validate("vlad likes potatoes");
        Assert.Equal("vlad likes potatoes",statement.LastValidatedPart);

        statement.Validate("dima dislikes carrots");
        Assert.Equal("dima dislikes carrots",statement.LastValidatedPart);
        
        statement.Validate("arina wants marshmallow");
        Assert.Equal("arina wants marshmallow",statement.LastValidatedPart);

        Assert.Throws<Exception>(()=>statement.Validate("vlad hates grechka"));
        Assert.Throws<Exception>(()=>statement.Validate("arina sent message"));
    }
    [Fact]
    public void ZeroOrMany(){
        var ident = new Term("<ident>",v=>{
            var identLength = v.TakeWhile(c=>char.IsLetter(c) || c=='_').Count();
            if(identLength==0){
                throw new Exception("ident must have name that can contain letters and underscore _");
            }
            return v[0..identLength];
        });
        var type = new Term("<type>",v=>{
            if(v[..3]=="int") return "int";
            if(v[..5]=="float") return "float";
            if(v[..6]=="string") return "string";
            throw new Exception("Type must be one of ('int','float','string')");
        });
        var varTerm = Term.OfConstant("var");
        var eq = Term.OfConstant(":=");
        var comma = Term.OfConstant(",");

        var varDef = 
            varTerm
            .Follows(ident)
            .Follows(eq)
            .Follows(type)
            .Follows(
                comma
                .Follows(ident)
                .Follows(eq)
                .Follows(type)
                .ZeroOrMany()
            );
        varDef.Validate("\n var  a:=int");
        Assert.Equal("var  a:=int",varDef.LastValidatedPart);

        varDef.Validate("var     a:=int, b:=float");
        Assert.Equal("var     a:=int, b:=float",varDef.LastValidatedPart);
        
        varDef.Validate("var bfg:=int,   dad:=float,dad:=string, dad:=int");
        Assert.Equal("var bfg:=int,   dad:=float,dad:=string, dad:=int",varDef.LastValidatedPart);

        //missing var
        Assert.Throws<Exception>(()=>varDef.Validate("a:=int,b:=float"));
        
        //unknown long type, so last pattern does not match
        varDef.Validate("var a:=int,b:=long");
        Assert.Equal("var a:=int",varDef.LastValidatedPart);
        
        //unkown symbol =, so the pattern does not match
        varDef.Validate("var a:=int,b:=int,c=float");
        Assert.Equal("var a:=int,b:=int",varDef.LastValidatedPart);
        
        // // unused commas
        varDef.Validate("var a:=int,b:=int,,,");
        Assert.Equal("var a:=int,b:=int",varDef.LastValidatedPart);

        // // unused comma
        varDef.Validate("var a:=int,b:=int,");
        Assert.Equal("var a:=int,b:=int",varDef.LastValidatedPart);
    }
    [Fact]
    public void OfConstant(){
        var program = Term.OfConstant("program");
        var name = new Term("<ident>",v=>{
            var identLength = v.TakeWhile(c=>char.IsLetter(c) || c=='_').Count();
            if(identLength==0){
                throw new Exception("ident must have name that can contain letters and underscore _");
            }
            return v[0..identLength];
        });
        var open = Term.OfConstant("(");
        var close = Term.OfConstant(")");
        var programDef =
            name.Follows(program)
            .Follows(
                open
                .Follows(name)
                .Follows(close)
            );
        programDef.Validate("   int program(type)");
        programDef.Validate("\n void  program (string) ");

        Assert.Throws<Exception>(()=>programDef.Validate("main(type) yey"));
        Assert.Throws<Exception>(()=>programDef.Validate("type123 func (type)"));
    }
    [Fact]
    public void Follows()
    {
        var t1 = new Term("<Name>",v=>
            char.IsUpper(v[0]) ? 
            new string(v.TakeWhile(v=>char.IsLetter(v)).ToArray()) : 
            throw new Exception("Name must start with uppercase")
        );
        var t2 = new Term("<Time>",v=>{
            if(v[..2]=="is") return "is";
            if(v[..3]=="was") return "was";
            if(v[..7]=="will be") return "will be";
            throw new Exception("Time must be one of ('is','was','will be')");
        });
        var t3 = new Term("<Mood>",v=>{
            if(v[..3]=="sad") return "sad";
            if(v[..5]=="happy") return "happy";
            if(v[..7]=="working") return "working";
            throw new Exception($"Mood must be one of ('sad','happy','working') '{v}' is given");
        });
        var together = t1.Follows(t2).Follows(t3);
        together.Validate(" Vlad was happy");
        together.Validate("Nina is sad");
        together.Validate("Nikita  will be   working");
        Assert.Throws<Exception>(()=>together.Validate("small name"));
        Assert.Throws<Exception>(()=>together.Validate("GoodName was unknownmood"));
        Assert.Throws<Exception>(()=>together.Validate("GoodName badtime sad"));
    }
}