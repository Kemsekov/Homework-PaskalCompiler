using System.Diagnostics;
using Eto.Parse;
using Microsoft.AspNetCore.Http;
using Modules;

namespace tests;

public class MyGrammarTests
{
    [Fact]
    public void OrPriority()
    {
        var letter = new MyGrammar("<letter>", Terminals.Letter);
        var digit = new MyGrammar("<digit>", Terminals.Digit);

        //<number> ::= <digit>{<digit>}
        var number = (digit & digit.Repeat(0)).WithName("<number>");
        //<name> ::= <letter>{<letter>}
        var name = (letter & letter.Repeat(0)).WithName("<name>");

        //<variable> ::= <name> | <name>_<number>
        var variable1 = name | name & "_" & number;

        //<variable> ::= <name>_<number> | <name>
        var variable2 = name & "_" & number | name;

        var input = "vlad_123";
        //will handle shorter term first
        var res1 = new Grammar(variable1).Match("variable1");
        System.Console.WriteLine('a');
        // Assert.Equal("vlad",variable1.LastValidatedPart);
        // //will handle longer term first
        // var res2 = variable2.Validate(input);
        // Assert.Equal(input,variable2.LastValidatedPart);

    }
    // [Fact]
    // public void OfSelf()
    // {
    //     var letter = new MyGrammar("<letter>", s => char.IsLetter(s[0]) ? s[0..1] : throw new Exception("not a letter"));
    //     var digit = new MyGrammar("<digit>", s => char.IsDigit(s[0]) ? s[0..1] : throw new Exception("not a digit"));

    //     //<number> ::= <digit>{<digit>}
    //     var number = (digit & digit.ZeroOrMany()).WithName("<number>");
    //     number.Validate("432535 name1");
    //     Assert.Equal("432535", number.LastValidatedPart);
    //     Assert.Throws<MyGrammarException>(() => number.Validate("abcde"));

    //     var op = MyGrammar.OfMany("<op>", ["+", "-"]);

    //     // it is a bit tricky not to stuck into infinite recursion
    //     // you just need to define some logic before recursion hits
    //     // <expression> ::= <number><op><number> | (<expression>)
    //     var expression =
    //         MyGrammar
    //         .OfSelf_(t =>number & op & number | ("(" & t & ")"))
    //         .WithName("<expression>");
    //     expression.Validate("123+86");
    //     Assert.Equal("123+86", expression.LastValidatedPart);

    //     expression.Validate("94-388");
    //     Assert.Equal("94-388", expression.LastValidatedPart);


    //     expression.Validate("(119-53)");
    //     Assert.Equal("(119-53)", expression.LastValidatedPart);

    //     expression.Validate("(((119-53)))");
    //     Assert.Equal("(((119-53)))", expression.LastValidatedPart);

    //     //must throws on uneven amount of brackets
    //     Assert.Throws<MyGrammarException>(() => expression.Validate("((1+43)"));
    // }
}