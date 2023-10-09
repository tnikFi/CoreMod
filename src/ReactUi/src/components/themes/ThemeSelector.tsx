import { IconButton, Tooltip } from '@mui/material'
import React from 'react'
import LightModeIcon from '@mui/icons-material/LightMode'
import DarkModeIcon from '@mui/icons-material/DarkMode'
import BrightnessAutoIcon from '@mui/icons-material/BrightnessAuto'
import { ColorSchemeContext } from '../../contexts/ColorSchemeContext'

const ThemeSelector = () => {
  const { colorScheme, setColorScheme } = React.useContext(ColorSchemeContext)

  const setNextColorScheme = React.useCallback(() => {
    setColorScheme(colorScheme === 'system' ? 'dark' : colorScheme === 'dark' ? 'light' : 'system')
  }, [colorScheme, setColorScheme])

  return (
    <Tooltip title="Toggle theme">
      <IconButton onClick={setNextColorScheme}>
        {colorScheme === 'system' && <BrightnessAutoIcon />}
        {colorScheme === 'light' && <LightModeIcon />}
        {colorScheme === 'dark' && <DarkModeIcon />}
      </IconButton>
    </Tooltip>
  )
}

export default ThemeSelector
