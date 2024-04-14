import React from 'react'
import { GuildDto, GuildPermissions, RoleDto } from '../api'

export interface ISelectedGuildContext {
  selectedGuild: GuildDto | null
  guildPermissions: GuildPermissions | undefined
  publicRoles: RoleDto[] | undefined
}

export const SelectedGuildContext = React.createContext<ISelectedGuildContext>({
  selectedGuild: null,
  guildPermissions: undefined,
  publicRoles: undefined,
})
