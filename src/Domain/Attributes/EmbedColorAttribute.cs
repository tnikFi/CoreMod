using Discord;

namespace Domain.Attributes;

/// <summary>
///     Specifies the embed color for messages related to the moderation type
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class EmbedColorAttribute : Attribute
{
    public Color Color { get; }
    
    public EmbedColorAttribute(int r, int g, int b)
    {
        Color = new Color(r, g, b);
    }
}