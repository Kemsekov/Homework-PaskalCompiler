using static AppInstance;

args=["examples/input.pas"];

if(args.Length==0){
    System.Console.WriteLine("Specify file to compile");
    return;
}
if(!File.Exists(args[0])){
    System.Console.WriteLine("File not found");
    return;
}
Args=args;
var builder = CreateHostBuilder(args);
builder.ConfigureServices(Configure);
App = builder.Build();


App.StartAsync().Wait();
