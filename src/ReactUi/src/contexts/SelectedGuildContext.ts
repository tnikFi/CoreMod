import React from 'react'
import { GuildDto, GuildPermissions } from '../api'

export interface ISelectedGuildContext {
  selectedGuild: GuildDto | null
  guildPermissions: GuildPermissions | undefined
}

export const SelectedGuildContext = React.createContext<ISelectedGuildContext>({
  selectedGuild: null,
  guildPermissions: undefined,
})
