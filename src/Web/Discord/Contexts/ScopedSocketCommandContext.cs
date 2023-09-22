using Discord.Commands;
using Discord.WebSocket;

namespace Web.Discord.Contexts;

public class ScopedSocketCommandContext : SocketCommandContext, IDisposable
{
    public IServiceScope ServiceScope { get; }
    
    public ScopedSocketCommandContext(IServiceScope serviceScope, DiscordSocketClient client, SocketUserMessage msg) :
        base(client, msg)
    {
        ServiceScope = serviceScope;
    }
    
    public void Dispose() { }
}