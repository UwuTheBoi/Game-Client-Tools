// Copyright (c) 2023 UwuTheBoi.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools.Client;
using Tools.Client.ClientCrypt;
using Tools.Client.Diablo;
using Tools.Client.Services;
using Tools.Client.Wow.Windows;

var sw = Stopwatch.StartNew();

// Little workaround to get the encrypt mode early.
// TODO: make it pretty.
var commandLineOptions = new CommandLineOptions();
var encryptMode = false;
var game = Game.None;

commandLineOptions.RootCommand.SetHandler(context =>
{
    encryptMode = context.ParseResult.GetValueForOption(commandLineOptions.GameEncryptMode);
    game = context.ParseResult.GetValueForOption(commandLineOptions.Game);
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

        _ = game switch
        {
            Game.Diablo => services.AddScoped<IPatterns, DiabloPatterns>(),
            Game.Overwatch => throw new NotImplementedException(),
            Game.Wow => services.AddScoped<IPatterns, WowPatterns>(),
            _ => throw new NotImplementedException()
        };

        services.AddScoped<IGameClientCryptHelper, WindowsGameClientCryptHelper>();
        services.AddScoped<GameClientCrypt>(serviceProvider => new(encryptMode));
        services.AddScoped<GameClientCryptService>();
    }).Build();

var service = host.Services.GetRequiredService<GameClientCryptService>();

service.Run(args);

Console.WriteLine();
Console.WriteLine($"Finished in {sw.Elapsed.TotalSeconds} seconds.");
