
public class ErrorDescriptions{
    public ErrorDescriptions(IDictionary<long,string> descriptions)
    {
        Descriptions=descriptions;
    }
    public IDictionary<long,string> Descriptions;
    public string this[long errorCode]=>Descriptions[errorCode];
}
