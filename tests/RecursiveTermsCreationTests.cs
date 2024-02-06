
namespace tests;

public class RecursiveTermsCreationTests
{
    Term Letter() => new Term("letter", s => char.IsLetter(s[0]) ? s[0..1] : throw new Exception("Letter expected"));
    Term Digit() => new Term("digit", s => char.IsDigit(s[0]) ? s[0..1] : throw new Exception("Digit expected"));
    [Fact]
    public void SimpleExpression()
    {
        var terms = new RecursiveTermsCreation();
        terms.Add("digit", Digit);
        terms.Add("number", () => terms["digit"] & terms["digit"].ZeroOrMany());
        var op = (Term) "*" | "+";

        terms.Add("expr", () => 
            terms["number"] & 
            (op & terms["number"]).ZeroOrMany() |
            "(" & terms["expr"] & ")");

        terms.Add("long expr",()=>
            terms["expr"] & 
            (op & terms["expr"]).ZeroOrMany() |
            "(" & terms["long expr"] & ")"
        );
        var expr = terms["long expr"];

        var good = new[]{
            "918223",
            "((((1))))+2",
            "123+938",
            "98*1245",
            "18+98*33",
            "66*1+4",
            "(11)",
            "(1+11)",
            "(1+11)*9",
            "7+12*(1+11)*9+92",
            "1+(2*(3+4))+(5*(6+7))",
        };
        foreach(var t in good){
            expr.Validate(t);
            Assert.Equal(t,expr.Matches.Match);
        }

        var bad = new[]{
            "*918223",
            "123+938+",
            "+98*1245",
            "98(*1245)",
            "(12+82*2",
            "12+82*2)",
        };
        foreach(var t in bad){
            Assert.Throws<TermException>(()=>expr.Validate(t));
        }
    }
    [Fact]
    public void SimpleAdd()
    {
        var terms = new RecursiveTermsCreation();
        terms.Add("word", () => terms["letter"] & terms["letter"].ZeroOrMany());
        terms.Add("number", () => terms["digit"] & terms["digit"].ZeroOrMany());
        terms.Add("name", () => terms["word"] & "_" & terms["number"]);

        terms.Add("letter", Letter);
        terms.Add("digit", Digit);


        var num = terms["number"];
        num.Validate("1234");

        var name = terms["name"];

        name.Validate("vlad_5412");
        Assert.Equal("vlad_5412", name.Matches.Match);

        name.Validate("Dima_829_le");
        Assert.Equal("Dima_829", name.Matches.Match);

    }
}