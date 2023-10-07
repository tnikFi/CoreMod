import {
  Avatar,
  Box,
  List,
  ListItem,
  ListItemAvatar,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  SwipeableDrawer,
} from '@mui/material'
import React from 'react'
import { AuthContext } from 'react-oauth2-code-pkce'
import LogoutIcon from '@mui/icons-material/Logout'
import { useNavigate } from 'react-router-dom'

interface UserMenuProps {
  open: boolean
  onClose: () => void
  onOpen: () => void
}

const UserDrawer = ({ open, onClose, onOpen }: UserMenuProps) => {
  const { logOut, idTokenData } = React.useContext(AuthContext)
  const navigate = useNavigate()

  const userName = idTokenData?.userName
  const avatar = idTokenData?.avatar

  const handleLogout = React.useCallback(() => {
    logOut()
    navigate('/')
  }, [logOut, navigate])

  return (
    <>
      <SwipeableDrawer
        open={open}
        anchor="right"
        onClose={onClose}
        onOpen={onOpen}
        sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
      >
        <Box sx={{ width: 250 }} role="presentation" onClick={onClose} onKeyDown={onClose}>
          <List>
            <ListItem>
              <ListItemAvatar>
                <Avatar src={avatar} />
              </ListItemAvatar>
              <ListItemText primary={userName} />
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
