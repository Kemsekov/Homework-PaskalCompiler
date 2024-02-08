/// <summary>
/// Contains constants for Pascal symbols
/// </summary>
public static class Lexical
{
    public const byte
        undefined = 0,
        star = (byte)'*', // *
        slash = (byte)'/', // /
        equal = (byte)'=', // =
        comma = (byte)',', // ,
        semicolon = (byte)';', // ;
        colon = (byte)':', // :
        point = (byte)'.', // .
        arrow = (byte)'^', // ^
        leftpar = (byte)'(',    // (
        rightpar = (byte)')',   // )
        lbracket = (byte)'[',  // [
        rbracket = (byte)']',  // ]
        flpar = (byte)'{', // {
        frpar = (byte)'}', // }
        later = (byte)'<', // <
        greater = (byte)'>',   // >
        laterequal = 254,    //  <=
        greaterequal = 253,  //  >=
        latergreater = 252,  //  <>
        plus = (byte)'+',  // +
        minus = (byte)'-', // –
        lcomment = 251,  //  (*
        rcomment = 250,  //  *)
        assign = 249,    //  :=
        twopoints = 248, //  ..
        ident = 247,  // идентификатор
        stringc = 246,  // строковая константа
        floatc = 245,// вещественная константа
        intc = 244,  // целая константа
        casesy = 243,
        elsesy = 242,
        filesy = 241,
        gotosy = 240,
        thensy = 239,
        typesy = 238,
        untilsy = 237,
        dosy = 236,
        withsy = 235,
        ifsy = 234,
        insy = 233,
        ofsy = 232,
        orsy = 231,
        tosy = 230,
        endsy = 229,
        varsy = 228,
        divsy = 227,
        andsy = 226,
        notsy = 225,
        forsy = 224,
        modsy = 223,
        nilsy = 222,
        setsy = 221,
        beginsy = 220,
        whilesy = 219,
        arraysy = 218,
        constsy = 217,
        labelsy = 216,
        downtosy = 215,
        packedsy = 214,
        recordsy = 213,
        repeatsy = 212,
        programsy = 211,
        functionsy = 210,
        proceduresy = 209;
}