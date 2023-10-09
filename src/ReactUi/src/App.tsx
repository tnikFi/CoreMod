import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './pages/Layout'
import Home from './pages/Home'
import NotFound from './pages/NotFound'
import Panel from './pages/panel/Panel'
import Overview from './pages/panel/Overview'
import { CssBaseline, ThemeProvider, createTheme, useMediaQuery } from '@mui/material'
import React from 'react'
import { ColorScheme, ColorSchemeContext } from './contexts/ColorSchemeContext'
import { useLocalStorage } from 'usehooks-ts'

const App = () => {
  const [colorScheme, setColorScheme] = useLocalStorage<ColorScheme>('colorScheme', 'system')
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)')

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
            </Route>
            <Route path="panel" element={<Panel />}>
              <Route path="" element={<Overview />} />
            </Route>
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </ColorSchemeContext.Provider>
  )
}

export default App
