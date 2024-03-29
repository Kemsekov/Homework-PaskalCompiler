
public static class Extensions{
    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> values, Predicate<T> where){
        var buffer = new List<T>();
        foreach(var v in values){
            if(!where(v)) buffer.Add(v);
            else{
                yield return buffer.ToList();
                buffer.Clear();
            }
        }
        if(buffer.Count!=0) yield return buffer;
    }
    
}
