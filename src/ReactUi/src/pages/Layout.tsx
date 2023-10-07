import {
  AppBar,
  Avatar,
  Box,
  Button,
  Container,
  IconButton,
  Menu,
  MenuItem,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material'
import React from 'react'
import MenuIcon from '@mui/icons-material/Menu'
import { Outlet, useNavigate } from 'react-router-dom'
import { AuthContext } from 'react-oauth2-code-pkce'
import AuthenticatedComponent from '../components/authentication/AuthenticatedComponent'
import UnauthenticatedComponent from '../components/authentication/UnauthenticatedComponent'
import { OpenAPI } from '../api'

const pages = [
  {
    name: 'Home',
    path: '/',
    requireAuth: false,
  },
  {
    name: 'Panel',
    path: '/panel',
    requireAuth: true,
  },
]

export interface JwtClaims {
  userId: string
  userName: string
  avatar: string
  exp: number
  iss: string
  aud: string
}

const Layout = () => {
  const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null)
  const [anchorElUser, setAnchorElUser] = React.useState<null | HTMLElement>(null)
  const navigate = useNavigate()
  const { login, logOut, idToken, idTokenData } = React.useContext(AuthContext)

  React.useEffect(() => {
    OpenAPI.TOKEN = idToken
  }, [idToken])

  const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElNav(event.currentTarget)
  }

  const handleCloseNavMenu = (path?: string) => {
    setAnchorElNav(null)
    if (path) {
      navigate(path)
    }
  }

  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget)
  }

  const handleCloseUserMenu = () => {
    setAnchorElUser(null)
  }

  const claims = idTokenData as JwtClaims
  const avatar = claims?.avatar
  const userName = claims?.userName

  return (
    <>
      <AppBar position="static">
        <Container maxWidth={false}>
          <Toolbar disableGutters>
            <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
              <IconButton
                size="large"
                aria-label="nav menu"
                aria-controls="menu-appbar"
                aria-haspopup="true"
                onClick={handleOpenNavMenu}
                color="inherit"
              >
                <MenuIcon />
              </IconButton>
              <Menu
                id="menu-appbar"
                anchorEl={anchorElNav}
                anchorOrigin={{
                  vertical: 'bottom',
                  horizontal: 'left',
                }}
                keepMounted
                transformOrigin={{
                  vertical: 'top',
                  horizontal: 'left',
                }}
                open={Boolean(anchorElNav)}
                onClose={() => handleCloseNavMenu()}
                sx={{
                  display: { xs: 'block', md: 'none' },
                }}
              >
                {pages.map(
                  (page) =>
                    (page.requireAuth === false || idToken) && (
                      <MenuItem key={page.name} onClick={() => handleCloseNavMenu(page.path)}>
                        <Typography textAlign="center">{page.name}</Typography>
                      </MenuItem>
                    )
                )}
              </Menu>
            </Box>

            <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
              {pages.map(
                (page) =>
                  (page.requireAuth === false || idToken) && (
                    <Button
                      key={page.name}
                      onClick={() => handleCloseNavMenu(page.path)}
                      sx={{ my: 2, color: 'white', display: 'block' }}
                    >
                      {page.name}
                    </Button>
                  )
              )}
            </Box>
            <UnauthenticatedComponent>
              <Button color="inherit" onClick={() => login()}>
                Login
              </Button>
            </UnauthenticatedComponent>
            <AuthenticatedComponent>
              <Box sx={{ flexGrow: 0 }}>
                <Tooltip title="User settings">
                  <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                    <Avatar alt={userName} src={avatar ?? ''} />
                  </IconButton>
                </Tooltip>
                <Menu
                  sx={{ mt: '45px' }}
                  id="menu-appbar"
                  anchorEl={anchorElUser}
                  anchorOrigin={{
                    vertical: 'top',
                    horizontal: 'right',
                  }}
                  keepMounted
                  transformOrigin={{
                    vertical: 'top',
                    horizontal: 'right',
                  }}
                  open={Boolean(anchorElUser)}
                  onClose={handleCloseUserMenu}
                >
                  <MenuItem
                    onClick={() => {
                      handleCloseUserMenu()
                      logOut()
                    }}
                  >
                    Logout
                  </MenuItem>
                </Menu>
              </Box>
            </AuthenticatedComponent>
          </Toolbar>
        </Container>
      </AppBar>
      <Outlet />
    </>
  )
}

export default Layout
