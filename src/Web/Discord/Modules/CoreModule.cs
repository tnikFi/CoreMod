using System.Diagnostics;
using System.Reflection;
using System.Text;
using Discord;
using Discord.Commands;
using Infrastructure.Configuration;

namespace Web.Discord.Modules;

[Name("Core")]
[Summary("Core commands")]
public class CoreModule : ModuleBase<SocketCommandContext>
{
    public CommandService Commands { get; set; } = null!;

    [Command("ping")]
    [Summary("Responds with \"Pong!\"")]
    public async Task PingAsync()
    {
        await ReplyAsync("Pong!", messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("help")]
    [Summary("Shows help for commands")]
    public async Task HelpAsync([Summary("Module or command search term")] string? query = null)
    {
        var embedBuilder = new EmbedBuilder();

        if (query is null)
        {
            embedBuilder = embedBuilder.WithTitle("Help System");
            embedBuilder = embedBuilder.WithDescription("Displays help for commands and modules.");
            embedBuilder = embedBuilder.AddField("Usage", 
                "Use `help <module>` to list commands in a module.\n" +
                "Use `help <command>` to get help for a command.");
            embedBuilder = embedBuilder.AddField("Modules",
                string.Join('\n', Commands.Modules.Select(x =>
                        $"- `{x.Name}`: {x.Summary ?? "No description available."}")));
        } else {
            // Find the complete match for the query
            var command = Commands.Commands.FirstOrDefault(x =>
                x.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
                || x.Aliases.Contains(query, StringComparer.OrdinalIgnoreCase));
            var module = Commands.Modules.FirstOrDefault(x =>
                x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));

            if (command is not null)
            {
                embedBuilder = embedBuilder.WithTitle("Command help: " + command.Name);
                embedBuilder = embedBuilder.WithDescription(command.Summary ?? "No description available.");
                var paramsString = string.Join(" ", command.Parameters.Select(x => $"[{x.Name}" + (x.IsOptional ? "?]" : "]")));
                var usage = command.Name;
                if (!string.IsNullOrEmpty(paramsString))
                {
                    usage += " " + paramsString;
                }
                embedBuilder.AddField("Usage", $"`{usage}`");
                if (command.Parameters.Any())
                {
                    var parameters = new StringBuilder();
                    foreach (var parameter in command.Parameters)
                    {
                        var summary = parameter.Summary ?? "No description available.";
                        if (parameter.IsOptional)
                        {
                            summary = $"Optional. {summary}";
                        }
                        parameters.AppendLine($"- `{parameter.Name}`: {summary}");
                    }
                    embedBuilder.AddField("Parameters", parameters.ToString());
                }
                
                // Get the command aliases, excluding the command name
                var aliases = command.Aliases.Where(x => x != command.Name).ToArray();
                if (aliases.Any())
                {
                    embedBuilder.AddField("Aliases", string.Join('\n', aliases));
                }
                
                // Add remarks if available
                if (!string.IsNullOrEmpty(command.Remarks))
                {
                    embedBuilder.AddField("Remarks", command.Remarks);
                }
            }
            else if (module is not null)
            {
                embedBuilder = embedBuilder.WithTitle("Module help: " + module.Name);
                embedBuilder = embedBuilder.WithDescription(module.Summary ?? "No description available.");
                embedBuilder.AddField("Commands",
                    string.Join('\n', module.Commands.Select(x =>
                        $"- `{x.Name}`: {x.Summary ?? "No description available."}")));
            }
            else
            {
                await ReplyAsync("No command or module found matching the query.");
                return;
            }
        }
        
        await ReplyAsync(embed: embedBuilder.Build());
    }

    [Command("info")]
    [Summary("Shows information about the bot")]
    [Alias("about")]
    public async Task InfoAsync()
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("About")
            .WithDescription("A bot for moderating.");
        
        var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
        embedBuilder.AddField("Uptime", uptime.ToString(@"dd\.hh\:mm\:ss"));
        
        var buildDate = File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location);
        embedBuilder.AddField("Build date", buildDate.ToString("yyyy-MM-dd HH:mm:ss"));
        
        await ReplyAsync(embed: embedBuilder.Build());
    }
}