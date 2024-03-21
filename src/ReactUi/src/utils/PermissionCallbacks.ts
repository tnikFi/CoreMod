/**
 * @file PermissionCallbacks.ts
 * @description Reusable permission callbacks for checking if a user has permission to perform specific actions.
 */

import { GuildPermissions } from '../api'

/**
 * A callback that determines if the user has permission to view a page based on their guild permissions.
 */
export type GuildPermissionCallback = (permissions: GuildPermissions | undefined) => boolean

/**
 * Returns true if the user has permission to view the moderation page in the control panel.
 * @param permissions
 * @returns
 */
const viewModerationPage: GuildPermissionCallback = (permissions) => {
  if (!permissions) return false
  return (
    (permissions.administrator ||
      permissions.banMembers ||
      permissions.kickMembers ||
      permissions.moderateMembers ||
      permissions.manageRoles ||
      permissions.viewAuditLog) ??
    false
  )
}

/**
 * Returns true if the user has permission to view the settings page in the control panel.
 * @param permissions
 * @returns
 */
const viewSettingsPage: GuildPermissionCallback = (permissions) => permissions?.manageGuild ?? false

export const permissionCallbacks = {
  viewModerationPage,
  viewSettingsPage,
}
