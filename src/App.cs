using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools.Client;
using Tools.Client.Wow;
using Tools.Client.Wow.Windows;

var sw = Stopwatch.StartNew();

// Little workaround to get the encrypt mode early.
// TODO: make it pretty.
var commandLineOptions = new CommandLineOptions();
var encryptMode = false;

commandLineOptions.RootCommand.SetHandler(context =>
{
    encryptMode = context.ParseResult.GetValueForOption(commandLineOptions.GameEncryptMode);
});

var exitCode = await commandLineOptions.Instance.InvokeAsync(args);

if (exitCode != 0)
{
    Console.ReadKey();
    return;
}

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context, builder) => builder.ClearProviders())
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<CommandLineOptions>();
        services.AddScoped<IGameClientCryptHelper, GameClientCryptHelper>();
        services.AddScoped<GameClientCrypt>(serviceProvider => new(encryptMode));
        services.AddScoped<GameClientCryptService>();
    }).Build();

var service = host.Services.GetRequiredService<GameClientCryptService>();

service.Run(args);

Console.WriteLine();
Console.WriteLine($"Finished in {sw.Elapsed.TotalSeconds} seconds.");
