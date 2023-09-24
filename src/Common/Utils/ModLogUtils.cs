using Discord;
using Domain.Enums;

namespace Common.Utils;

public static class ModLogUtils
{
    public static Color GetEmbedColorForModerationType(ModerationType type)
        => type switch
        {
            ModerationType.Warning => Color.Orange,
            ModerationType.Mute => Color.DarkOrange,
            ModerationType.Kick => Color.DarkOrange,
            ModerationType.Ban => Color.Red,
            ModerationType.Unmute => Color.Green,
            ModerationType.Unban => Color.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}