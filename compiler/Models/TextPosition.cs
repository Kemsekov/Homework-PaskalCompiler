public struct TextPosition{
    public ulong LineNumber;
    public int CharNumber;
    public bool SameAs(TextPosition pos){
        return pos.LineNumber==LineNumber && pos.CharNumber == CharNumber;
    }
}
