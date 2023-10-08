import { PaletteMode } from '@mui/material'
import React from 'react'

export type ColorScheme = PaletteMode | 'system'

export interface IColorSchemeContext {
  colorScheme?: ColorScheme
  setColorScheme: (colorScheme: ColorScheme) => void
}

export const ColorSchemeContext = React.createContext<IColorSchemeContext>({
  colorScheme: 'system',
  setColorScheme: () => {},
})
