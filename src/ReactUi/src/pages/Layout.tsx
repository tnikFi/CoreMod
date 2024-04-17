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
} from '@mui/material'
import React from 'react'
import HomeIcon from '@mui/icons-material/Home'
import ViewComfyIcon from '@mui/icons-material/ViewComfy'
import MenuIcon from '@mui/icons-material/Menu'
import { Outlet, useNavigate } from 'react-router-dom'
import { AuthContext } from 'react-oauth2-code-pkce'
import AuthenticatedComponent from '../components/authentication/AuthenticatedComponent'
import UnauthenticatedComponent from '../components/authentication/UnauthenticatedComponent'
import { OpenAPI } from '../api'
import { NavDrawer, Page } from '../components/navigation/NavDrawer'
import ThemeSelector from '../components/themes/ThemeSelector'

const pages: Page[] = [
  {
    name: 'Home',
    path: '/',
    icon: <HomeIcon />,
    requireAuth: false,
  },
  {
    name: 'Control Panel',
    path: '/panel',
    icon: <ViewComfyIcon />,
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
  const [navOpen, setNavOpen] = React.useState(false)
  const [anchorElUser, setAnchorElUser] = React.useState<null | HTMLElement>(null)
  const { login, logOut, idToken, idTokenData } = React.useContext(AuthContext)
  const navigate = useNavigate()

  React.useEffect(() => {
    OpenAPI.TOKEN = idToken
  }, [idToken])

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
      <AppBar position="static" elevation={0} sx={{ backgroundColor: 'transparent' }}>
        <Container maxWidth={false}>
          <Toolbar disableGutters>
            <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
              <IconButton
                size="large"
                aria-label="nav menu"
                aria-controls="menu-appbar"
                aria-haspopup="true"
                onClick={() => setNavOpen(true)}
              >
                <MenuIcon />
              </IconButton>
            </Box>

            <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
              {pages.map(
                (page) =>
                  (page.requireAuth === false || idToken) && (
                    <Button
                      key={page.name}
                      onClick={() => navigate(page.path)}
                      sx={{ my: 2, color: 'text.primary', display: 'block' }}
                      variant="text"
                    >
                      {page.name}
                    </Button>
                  )
              )}
            </Box>
            <Box sx={{ flexGrow: 0, gap: 2, display: 'flex' }}>
              <ThemeSelector />
              <UnauthenticatedComponent>
                <Button sx={{ color: 'text.primary' }} onClick={() => login()}>
                  Login
                </Button>
              </UnauthenticatedComponent>
              <AuthenticatedComponent>
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
              </AuthenticatedComponent>
            </Box>
          </Toolbar>
        </Container>
      </AppBar>

      <NavDrawer
        pages={pages}
        open={navOpen}
        onClose={() => setNavOpen(false)}
        onOpen={() => setNavOpen(true)}
        sx={{ display: { xs: 'block', md: 'none' } }}
      />

      <Outlet />
    </>
  )
}

export default Layout
