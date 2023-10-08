import {
  Avatar,
  Box,
  Divider,
  List,
  ListItem,
  ListItemAvatar,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  SwipeableDrawer,
  Switch,
  useMediaQuery,
} from '@mui/material'
import React from 'react'
import { AuthContext } from 'react-oauth2-code-pkce'
import LogoutIcon from '@mui/icons-material/Logout'
import { useNavigate } from 'react-router-dom'
import { ColorSchemeContext } from '../../contexts/ColorSchemeContext'

interface UserMenuProps {
  open: boolean
  onClose: () => void
  onOpen: () => void
}

const UserDrawer = ({ open, onClose, onOpen }: UserMenuProps) => {
  const { logOut, idTokenData } = React.useContext(AuthContext)
  const { colorScheme, setColorScheme } = React.useContext(ColorSchemeContext)
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)')
  const navigate = useNavigate()

  const userName = idTokenData?.userName
  const avatar = idTokenData?.avatar

  const handleLogout = React.useCallback(() => {
    logOut()
    navigate('/')
  }, [logOut, navigate])

  const toggleSystemTheme = React.useCallback(() => {
    setColorScheme(colorScheme === 'system' ? (prefersDarkMode ? 'dark' : 'light') : 'system')
  }, [colorScheme, prefersDarkMode, setColorScheme])

  const toggleDarkTheme = React.useCallback(() => {
    setColorScheme(colorScheme === 'dark' ? 'light' : 'dark')
  }, [colorScheme, setColorScheme])

  // Prevent closing the drawer when using keyboard navigation (tab or shift+tab)
  const handleKeyDown: React.KeyboardEventHandler = React.useCallback(
    (e) => {
      if (e.key === 'Tab' || e.key === 'Shift') return
      onClose()
    },
    [onClose]
  )

  return (
    <>
      <SwipeableDrawer
        open={open}
        anchor="right"
        onClose={onClose}
        onOpen={onOpen}
        sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
      >
        <Box sx={{ width: 250 }} role="presentation" onClick={onClose} onKeyDown={handleKeyDown}>
          <List>
            <ListItem>
              <ListItemAvatar>
                <Avatar src={avatar} />
              </ListItemAvatar>
              <ListItemText primary={userName} />
            </ListItem>
            <ListItem onClick={(e) => e.stopPropagation()} onKeyDown={(e) => e.stopPropagation()}>
              <ListItemText primary="Use System Theme" />
              <Switch checked={colorScheme === 'system'} onChange={toggleSystemTheme} />
            </ListItem>
            <ListItem onClick={(e) => e.stopPropagation()} onKeyDown={(e) => e.stopPropagation()}>
              <ListItemText primary="Dark Mode" />
              <Switch
                checked={colorScheme === 'dark'}
                onChange={toggleDarkTheme}
                disabled={colorScheme === 'system'}
              />
            </ListItem>
            <Divider />
            <ListItem disablePadding>
              <ListItemButton onClick={() => {}}>
                <ListItemIcon>
                  <LogoutIcon />
                </ListItemIcon>
                <ListItemText primary="Dummy button" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton onClick={handleLogout}>
                <ListItemIcon>
                  <LogoutIcon />
                </ListItemIcon>
                <ListItemText primary="Logout" />
              </ListItemButton>
            </ListItem>
          </List>
        </Box>
      </SwipeableDrawer>
    </>
  )
}

export default UserDrawer
