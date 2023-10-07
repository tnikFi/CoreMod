import React from 'react'
import { GuildDto } from '../api'

export interface ISelectedGuildContext {
  selectedGuild: GuildDto | null
}

export const SelectedGuildContext = React.createContext<ISelectedGuildContext>({
  selectedGuild: null,
})
