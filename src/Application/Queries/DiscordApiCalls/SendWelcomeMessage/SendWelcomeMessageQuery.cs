using Application.Extensions;
using Application.Queries.Configuration.GetWelcomeMessage;
using Discord;
using MediatR;

namespace Application.Queries.DiscordApiCalls.SendWelcomeMessage;

public class SendWelcomeMessageQuery : IRequest
{
    public IGuildUser User { get; set; }
}

public class SendWelcomeMessageQueryHandler : IRequestHandler<SendWelcomeMessageQuery>
{
    private readonly IMediator _mediator;

    public SendWelcomeMessageQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(SendWelcomeMessageQuery request, CancellationToken cancellationToken)
    {
        var welcomeMessage = await _mediator.Send(new GetWelcomeMessageQuery
            {
                GuildId = request.User.GuildId
            },
            cancellationToken);
        if (welcomeMessage is null)
            return;
        var formattedWelcomeMessage = welcomeMessage
            .Replace("{user}", request.User.Mention)
            .Replace("{guild}", request.User.Guild.Name);
        await request.User.TrySendMessageAsync(formattedWelcomeMessage);
    }
}