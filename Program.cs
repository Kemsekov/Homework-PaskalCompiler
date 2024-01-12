using static AppInstance;

var ERRMAX = ulong.Parse(Environment.GetEnvironmentVariable("ERRMAX") ?? "10");
var MAXLINE = ulong.Parse(Environment.GetEnvironmentVariable("MAXLINE") ?? "2048");
args=["input.pas"];

if(args.Length==0){
    System.Console.WriteLine("Specify file to compile");
    return;
}

Args=args;
var builder = CreateHostBuilder(args);
builder.ConfigureServices(Configure);
App = builder.Build();


App.StartAsync().Wait();
