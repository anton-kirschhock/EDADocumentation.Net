using EDADocumentation.Net.Commands;

using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<ParseDocsCommand>("parse");
});

return app.Run(args);
