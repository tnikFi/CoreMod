using Domain.Attributes;

namespace Domain.Enums;

public enum ModerationType : byte
{
    [EmbedColor(230, 126, 34)] Warning = 1,

    [EmbedColor(230, 126, 34)] [CanBeTemporary]
    Mute = 2,

    [EmbedColor(231, 76, 60)] Kick = 3,

    [EmbedColor(231, 76, 60)] [CanBeTemporary]
    Ban = 4,

    [EmbedColor(46, 204, 113)] Unmute = 5,

    [EmbedColor(46, 204, 113)] Unban = 6
}