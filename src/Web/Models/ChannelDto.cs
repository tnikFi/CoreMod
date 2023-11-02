using System.Text.Json.Serialization;
using Discord;

namespace Web.Models;

public class ChannelDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required ChannelTypeEnum Type { get; set; }
    
    public enum ChannelTypeEnum
    {
        Text,
        Voice,
        Forum
    }
    
    public static ChannelDto FromChannel(IChannel channel)
    {
        return new ChannelDto
        {
            Id = channel.Id.ToString(),
            Name = channel.Name,
            Type = channel switch
            {
                IVoiceChannel _ => ChannelTypeEnum.Voice,
                IForumChannel _ => ChannelTypeEnum.Forum,
                _ => ChannelTypeEnum.Text
            }
        };
    }
}