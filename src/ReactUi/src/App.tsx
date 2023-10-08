import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './pages/Layout'
import Home from './pages/Home'
import NotFound from './pages/NotFound'
import Panel from './pages/panel/Panel'
import Overview from './pages/panel/Overview'
import { CssBaseline, PaletteMode, ThemeProvider, createTheme, useMediaQuery } from '@mui/material'
import React from 'react'
import { ColorScheme, ColorSchemeContext } from './contexts/ColorSchemeContext'

const App = () => {
  const [colorScheme, setColorScheme] = React.useState<ColorScheme | undefined>(undefined)
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)')

  // Load color scheme from local storage on mount
  React.useEffect(() => {
    const colorScheme = localStorage.getItem('colorScheme') as PaletteMode
    setColorScheme(colorScheme ?? 'system')
  }, [])

  // Save color scheme to local storage
  React.useEffect(() => {
    if (colorScheme) localStorage.setItem('colorScheme', colorScheme)
  }, [colorScheme])

  const theme = React.useMemo(
    () =>
      createTheme({
        palette: {
          mode:
            colorScheme === 'system'
              ? prefersDarkMode
                ? 'dark'
                : 'light'
              : colorScheme === 'dark'
              ? 'dark'
              : 'light' ?? 'system',
          primary: {
            900: '#0017af',
            800: '#002fc1',
            700: '#003acc',
            600: '#1845d8',
            500: '#1f4ee4',
            400: '#4e6aea',
            300: '#7286ee',
            200: '#9ea8f2',
            100: '#c5caf7',
            50: '#e8eafc',
          },
          secondary: {
            main: '#00dbff',
            dark: '#00a4c7',
            light: '#73e6ff',
          },
        },
      }),
    [colorScheme, prefersDarkMode]
  )

  return (
    <ColorSchemeContext.Provider value={{ colorScheme, setColorScheme }}>
      <ThemeProvider theme={theme}>
        <BrowserRouter>
          <CssBaseline />
          <Routes>
            <Route element={<Layout />}>
              <Route path="/" element={<Home />} />
              <Route path="*" element={<NotFound />} />
            </Route>
            <Route path="panel" element={<Panel />}>
              <Route path="" element={<Overview />} />
              <Route path="*" element={<NotFound />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </ColorSchemeContext.Provider>
  )
}

export default App
