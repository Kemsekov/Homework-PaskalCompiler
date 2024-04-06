using Modules;
using Modules.Nodes;

namespace tests;

public class BaseNodeTests
{
    [Fact]
    public void NameMatch(){
        var start = new StartBlock();
        var a = new ActualParameter(start);
        Assert.Equal("ActualParameter",a.Name);
        Assert.Equal("StartBlock",start.Name);
    }
}