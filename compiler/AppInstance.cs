using System.Runtime.Intrinsics.Arm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules;

/// <summary>
/// Place that contains reference to global application running, so it could be accessed from everywhere
/// </summary>
public static class AppInstance
{
    public static IHost? App { get; set; }
    public static IServiceProvider? Services => App?.Services;
    public static string[] Args{get;set;} = [""];
    public static void Configure(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(s=>JsonErrorDescriptionsFactory.Create(File.ReadAllText("errors.json")));
        services.AddSingleton(s=>ProgramFileReader.ReadProgramFile(Args[0]));
        services.AddSingleton(s=>new ConfigurationVariables{
            ERRMAX=ulong.Parse(Environment.GetEnvironmentVariable("ERRMAX") ?? "20"),
            MAXLINE = ulong.Parse(Environment.GetEnvironmentVariable("MAXLINE") ?? "2048"),
            MAXINT = long.Parse(Environment.GetEnvironmentVariable("MAXINT") ?? Int16.MaxValue.ToString())
        });
        services.AddSingleton<InputOutput>();
        services.AddSingleton<LexicalAnalysis>();
        services.AddSingleton<SyntaxAnalysis>();
        services.AddHostedService<MainHostedService>();
    }
    public static IHostBuilder CreateHostBuilder(string[] args){
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureAppConfiguration(c=>{
            c.AddJsonFile("settings.json",optional:true,reloadOnChange:true);
        });
        
        return builder;
    }
}
