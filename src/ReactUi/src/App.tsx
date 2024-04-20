import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './pages/Layout'
import Home from './pages/Home'
import NotFound from './pages/NotFound'
import Overview from './pages/panel/Overview'
import { CssBaseline, ThemeProvider, createTheme, useMediaQuery } from '@mui/material'
import React from 'react'
import { ColorScheme, ColorSchemeContext } from './contexts/ColorSchemeContext'
import { useLocalStorage } from 'usehooks-ts'
import Moderation from './pages/panel/Moderation'
import { LocalizationProvider } from '@mui/x-date-pickers'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { SnackbarProvider } from 'notistack'
import { green } from '@mui/material/colors'
const Panel = React.lazy(() => import('./pages/panel/Panel'))

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
            main: green[500],
          },
          secondary: {
            main: green[700],
          },
        },
      }),
    [colorScheme, prefersDarkMode]
  )

  return (
    <SnackbarProvider>
      <LocalizationProvider dateAdapter={AdapterDayjs}>
        <ColorSchemeContext.Provider value={{ colorScheme, setColorScheme }}>
          <ThemeProvider theme={theme}>
            <BrowserRouter>
              <CssBaseline />
              <Routes>
                <Route element={<Layout />}>
                  <Route path="/" element={<Home />} />
                </Route>
                <Route path="panel" element={<React.Suspense>
                  <Panel />
                </React.Suspense>}>
                  <Route path="" element={<Overview />} />
                  <Route path="moderation" element={<Moderation />} />
                </Route>
                <Route path="*" element={<NotFound />} />
              </Routes>
            </BrowserRouter>
          </ThemeProvider>
        </ColorSchemeContext.Provider>
      </LocalizationProvider>
    </SnackbarProvider>
  )
}

export default App
