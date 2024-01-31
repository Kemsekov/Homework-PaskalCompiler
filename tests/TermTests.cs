using Modules;

namespace tests;

public class TermTests
{

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
        statement.Validate("dima dislikes carrots");
        statement.Validate("arina wants marshmallow");

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
        statement.Validate("dima dislikes carrots");
        statement.Validate("arina wants marshmallow");

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
        varDef.Validate("var     a:=int, b:=float");
        varDef.Validate("var bfg:=int,   dad:=float,dad:=string, dad:=int");

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