using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Tools.Client;

class CommandLineOptions
{
    public Option<Game> Game = new("--game", () => Tools.Client.Game.Wow);
    public Option<string> GameInput = new("--input") { IsRequired = true };
    public Option<string> GameOutput = new("--output") { IsRequired = true };
    public Option<bool> GameEncryptMode = new("--encrypt", () => false);

    public CommandLineOptions()
    {
        RootCommand = new("Client Tools")
        {
            Game, GameInput, GameOutput, GameEncryptMode
        };
    }

    public Parser Instance => new CommandLineBuilder(ConfigureCommandLine(RootCommand))
        .UseHelp()
        .UseParseDirective()
        .CancelOnProcessTermination()
        .UseParseErrorReporting()
        .UseSuggestDirective()
        .Build();

    public RootCommand RootCommand;

    Command ConfigureCommandLine(Command rootCommand)
    {
        GameInput.AddValidator(optionResult =>
        {
            string inFile = optionResult.GetValueOrDefault<string>()!;

            if (!File.Exists(inFile))
                optionResult.ErrorMessage = $"Provided input file '{inFile}' doesn't exist.";
        });

        GameInput.AddValidator(optionResult =>
        {
            string outFile = optionResult.GetValueOrDefault<string>()!;

            if (string.IsNullOrEmpty(outFile))
                optionResult.ErrorMessage = $"Output file is required.";
        });

        rootCommand.TreatUnmatchedTokensAsErrors = true;

        return rootCommand;
    }
}
