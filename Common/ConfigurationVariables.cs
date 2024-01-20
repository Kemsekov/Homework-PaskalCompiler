public class ConfigurationVariables{
    public ulong ERRMAX;
    public ulong MAXLINE;
    public long MAXINT;
}

public enum ErrorCodes{
    StringFormatting = 3,
    WrongVariableName = 4,
    UnrecognizedSymbol = 5,
    FloatConstantManyDots=6,
    IntOverflow=7,
    
}