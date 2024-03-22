using Discord;

namespace Common.Permissions;

public static class CoreModPermissions
{
    /// <summary>
    ///     Permissions that grant access to the moderation view.
    /// </summary>
    public const GuildPermission ViewModerations = GuildPermission.Administrator
                                                    | GuildPermission.KickMembers
                                                    | GuildPermission.BanMembers
                                                    | GuildPermission.ModerateMembers
                                                    | GuildPermission.ManageRoles
                                                    | GuildPermission.ViewAuditLog;

    /// <summary>
    ///     Permissions that grant access to the settings view and allow the user to change the settings.
    /// </summary>
    public const GuildPermission ManageSettings = GuildPermission.ManageGuild;
}