using System.Collections.Concurrent;
using Newtonsoft.Json;

public static class JsonErrorDescriptionsFactory{

    public static ErrorDescriptions Create(string jsonString)
    {
        var json = (dynamic)(JsonConvert.DeserializeObject(jsonString) ?? throw new ArgumentException("Cannot open/parse json")) ;
        var errors = new ConcurrentDictionary<long,string>();
        foreach(var l in json){
            var name = long.Parse(l.Name);
            var desc = (string)l.Value;
            errors[name]=desc;
        }
        return new(errors);
    }
}