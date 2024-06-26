import React from 'react'
import { Outlet, useNavigate } from 'react-router-dom'
import { GuildDto, GuildPermissions, GuildsService, OpenAPI, RoleDto, UserService } from '../../api'
import {
  AppBar,
  Avatar,
  Box,
  Button,
  Container,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material'
import HomeIcon from '@mui/icons-material/Home'
import ViewComfyIcon from '@mui/icons-material/ViewComfy'
import MenuIcon from '@mui/icons-material/Menu'
import ShieldIcon from '@mui/icons-material/Shield'
import SettingsIcon from '@mui/icons-material/Settings'
import { AuthContext } from 'react-oauth2-code-pkce'
import UserDrawer from '../../components/user/UserDrawer'
import RequireAuthenticated from '../../components/authentication/RequireAuthenticated'
import { NavDrawer, Page } from '../../components/navigation/NavDrawer'
import Guild from '../../components/navigation/Guild'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import ThemeSelector from '../../components/themes/ThemeSelector'
import { permissionCallbacks } from '../../utils/PermissionCallbacks'
import AddIcon from '@mui/icons-material/Add'

const pages: Page[] = [
  {
    name: 'Home',
    path: '/',
    icon: <HomeIcon />,
  },
  {
    name: 'Overview',
    path: '',
    icon: <ViewComfyIcon />,
    requireAuth: true,
  },
  {
    name: 'Moderation',
    path: 'moderation',
    icon: <ShieldIcon />,
    requireAuth: true,
    permissionCallback: permissionCallbacks.viewModerationPage,
  },
  {
    name: 'Settings',
    path: 'settings',
    icon: <SettingsIcon />,
    requireAuth: true,
    permissionCallback: permissionCallbacks.viewSettingsPage,
  },
]

const Panel = () => {
  const [navOpen, setNavOpen] = React.useState(false)
  const [userMenuOpen, setUserMenuOpen] = React.useState(false)
  const [guilds, setGuilds] = React.useState<GuildDto[]>([])
  const [selectedGuild, setSelectedGuild] = React.useState<GuildDto | null>(null)
  const [guildPermissions, setGuildPermissions] = React.useState<GuildPermissions | undefined>(
    undefined
  )
  const [publicRoles, setPublicRoles] = React.useState<RoleDto[] | undefined>(undefined)
  const { idToken, idTokenData } = React.useContext(AuthContext)
  const navigate = useNavigate()

  // Update the OpenAPI token when the idToken changes
  React.useEffect(() => {
    OpenAPI.TOKEN = idToken
  }, [idToken])

  // Get the user's mutual guilds with the bot when the component mounts
  React.useEffect(() => {
    async function getGuilds() {
      const guilds = await UserService.getApiUserGuilds()
      setGuilds(guilds)
    }
    getGuilds()
  }, [])

  // Attempt to get the selected guild from local storage or default to the first guild
  React.useEffect(() => {
    const selectedGuildId = localStorage.getItem('selectedGuild')
    let selectedGuild: GuildDto | null | undefined = null
    if (selectedGuildId) {
      selectedGuild = guilds.find((guild) => guild.id?.toString() === selectedGuildId)
    }
    setSelectedGuild(selectedGuild ?? guilds[0] ?? null)
  }, [guilds])

  // Save the selected guild to local storage when it changes or remove it if it is null
  React.useEffect(() => {
    if (selectedGuild) {
      localStorage.setItem('selectedGuild', selectedGuild.id?.toString() ?? '')
    } else {
      localStorage.removeItem('selectedGuild')
    }
  }, [selectedGuild])

  // Get the guild permissions when the selected guild changes
  React.useEffect(() => {
    async function getGuildPermissions() {
      setGuildPermissions(undefined)
      if (selectedGuild?.id) {
        const permissions = await UserService.getApiUserPermissions(selectedGuild.id)
        setGuildPermissions(permissions)
      }
    }
    getGuildPermissions()
  }, [selectedGuild])

  // Get the public roles when the selected guild changes
  React.useEffect(() => {
    async function getPublicRoles() {
      setPublicRoles(undefined)
      if (selectedGuild?.id) {
        const roles = await GuildsService.getApiGuildsPublicRoles(selectedGuild.id)
        setPublicRoles(roles)
      }
    }
    getPublicRoles()
  }, [selectedGuild])

  const userName = idTokenData?.userName
  const avatar = idTokenData?.avatar

  return (
    <RequireAuthenticated>
      <SelectedGuildContext.Provider
        value={{
          selectedGuild: selectedGuild,
          guildPermissions: guildPermissions,
          publicRoles: publicRoles,
        }}
      >
        <Box sx={{ display: { xs: 'block', md: 'flex', maxWidth: '100vw' } }}>
          <AppBar position="fixed" sx={{ display: { xs: 'block', md: 'none' } }}>
            <Container maxWidth={false}>
              <Toolbar disableGutters>
                <Box
                  sx={{
                    flexGrow: 1,
                    display: { xs: 'flex', md: 'none' },
                  }}
                >
                  <IconButton
                    size="large"
                    edge="start"
                    color="inherit"
                    aria-label="menu"
                    onClick={() => setNavOpen(true)}
                  >
                    <MenuIcon />
                  </IconButton>
                </Box>
                <Box sx={{ flexGrow: 0, gap: 2, display: 'flex' }}>
                  <ThemeSelector />
                  <Tooltip title="User settings">
                    <IconButton sx={{ p: 0 }} onClick={() => setUserMenuOpen(true)}>
                      <Avatar alt={userName} src={avatar ?? ''} />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Toolbar>
            </Container>
          </AppBar>

          <AppBar
            position="fixed"
            sx={{
              display: { xs: 'none', md: 'block' },
              zIndex: (theme) => theme.zIndex.drawer + 1,
            }}
          >
            <Container maxWidth={false}>
              <Toolbar disableGutters>
                <Box
                  sx={{
                    flexGrow: 1,
                    display: { xs: 'flex', md: 'none' },
                  }}
                >
                  <IconButton
                    size="large"
                    edge="start"
                    color="inherit"
                    aria-label="menu"
                    onClick={() => setNavOpen(true)}
                  >
                    <MenuIcon />
                  </IconButton>
                </Box>
                <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
                  {pages.map((page) => (
                    <Button
                      key={page.name}
                      sx={{ my: 2, color: 'white', display: 'block' }}
                      onClick={() => navigate(page.path)}
                      disabled={
                        page.requireAuth && page.permissionCallback
                          ? !page.permissionCallback(guildPermissions)
                          : false
                      }
                    >
                      <Typography textAlign="center" variant="button">
                        {page.name}
                      </Typography>
                    </Button>
                  ))}
                </Box>
                <Box sx={{ flexGrow: 0, gap: 2, display: 'flex' }}>
                  <ThemeSelector />
                  <Tooltip title="User settings">
                    <IconButton sx={{ p: 0 }} onClick={() => setUserMenuOpen(true)}>
                      <Avatar alt={userName} src={avatar ?? ''} />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Toolbar>
            </Container>
          </AppBar>

          <UserDrawer
            open={userMenuOpen}
            onClose={() => setUserMenuOpen(false)}
            onOpen={() => setUserMenuOpen(true)}
          />

          <NavDrawer
            pages={pages}
            open={navOpen}
            onClose={() => setNavOpen(false)}
            onOpen={() => setNavOpen(true)}
            sx={{ display: { xs: 'block', md: 'none' } }}
            basePath="/panel"
          >
            <Divider />
            <List>
              {guilds.map((guild) => (
                <Guild
                  key={guild.id}
                  name={guild.name ?? ''}
                  onClick={() => setSelectedGuild(guild)}
                  icon={guild.icon}
                  selected={guild.id === selectedGuild?.id}
                />
              ))}
            </List>
            <Divider />
            <ListItem disablePadding>
              <ListItemButton href={import.meta.env.VITE_INVITE_URL} target="_blank">
                <ListItemIcon>
                  <AddIcon />
                </ListItemIcon>
                <ListItemText primary={'Add to Server'} primaryTypographyProps={{ noWrap: true }} />
              </ListItemButton>
            </ListItem>
          </NavDrawer>

          <Drawer
            variant="permanent"
            sx={{
              width: 250,
              flexShrink: 0,
              [`& .MuiDrawer-paper`]: { width: 250, boxSizing: 'border-box' },
              display: { xs: 'none', md: 'block' },
            }}
          >
            <Toolbar />
            <List>
              {guilds.map((guild) => (
                <Guild
                  key={guild.id}
                  name={guild.name ?? ''}
                  onClick={() => setSelectedGuild(guild)}
                  icon={guild.icon}
                  selected={guild.id === selectedGuild?.id}
                />
              ))}
              <Divider />
              <ListItem disablePadding>
                <ListItemButton href={import.meta.env.VITE_INVITE_URL} target="_blank">
                  <ListItemIcon>
                    <AddIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary={'Add to Server'}
                    primaryTypographyProps={{ noWrap: true }}
                  />
                </ListItemButton>
              </ListItem>
            </List>
          </Drawer>

          <Box component="main" sx={{ flexGrow: 1, p: 3, minWidth: 0 }}>
            <Toolbar />
            <Outlet />
          </Box>
        </Box>
      </SelectedGuildContext.Provider>
    </RequireAuthenticated>
  )
}

export default Panel
