import {
  Box,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  SwipeableDrawer,
  SwipeableDrawerProps,
} from '@mui/material'
import React from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import AuthenticatedComponent from '../authentication/AuthenticatedComponent'
import { GuildPermissions } from '../../api'

export interface Page {
  name: string
  path: string
  requireAuth?: boolean
  icon?: React.ReactNode

  /**
   * A callback that determines if the user has permission to view the page.
   * 
   * Actions performed in the restricted page should still be validated on the server to prevent unauthorized access.
   * @param permissions The permissions the user has in the current guild. `undefined` if no guild is selected or if permissions have not been fetched.
   * @returns Boolean indicating if the user has permission to view the page.
   */
  permissionCallback?: (permissions?: GuildPermissions) => boolean
}

interface NavMenuProps extends SwipeableDrawerProps {
  pages: Page[]
  open: boolean
  onClose: () => void
  onOpen: () => void

  /**
   * The base path of the pages. Used to determine if a page is selected.
   */
  basePath?: string

  /**
   * The permissions the user has in the current guild. Used to determine if certain nav items should be disabled.
   * 
   * Only used if the page has a `permissionCallback` property and `requireAuth` is true.
   */
  guildPermissions?: GuildPermissions
}

interface NavListItemProps {
  page: Page
  currentPath: string
  navigate: (path: string) => void
  basePath?: string
  disabled?: boolean
}

/**
 * A nav list item that is always rendered.
 */
const NavListItem: React.FC<NavListItemProps> = ({ currentPath, navigate, page, basePath, disabled }) => {
  return (
    <ListItem key={page.name} disablePadding>
      <ListItemButton
        onClick={() => navigate(page.path)}
        selected={
          currentPath === `${basePath ?? ''}${page.path}` ||
          currentPath === `${basePath ?? ''}/${page.path}`
        }
        disabled={disabled}
      >
        {page.icon && <ListItemIcon>{page.icon}</ListItemIcon>}
        <ListItemText primary={page.name} />
      </ListItemButton>
    </ListItem>
  )
}

/**
 * A nav list item that is only rendered if the user is authenticated if the page requires authentication.
 */
const AuthenticatedNavListItem: React.FC<NavListItemProps> = ({
  currentPath,
  navigate,
  page,
  basePath,
  disabled,
}) => {
  return page.requireAuth ? (
    <AuthenticatedComponent>
      <NavListItem currentPath={currentPath} navigate={navigate} page={page} basePath={basePath} disabled={disabled} />
    </AuthenticatedComponent>
  ) : (
    <NavListItem currentPath={currentPath} navigate={navigate} page={page} basePath={basePath} disabled={disabled} />
  )
}

/**
 * A drawer used for navigating between pages. Children are rendered inside the drawer.
 */
export const NavDrawer: React.FC<React.PropsWithChildren<NavMenuProps>> = ({
  pages,
  open,
  onClose,
  onOpen,
  children,
  basePath,
  guildPermissions,
  ...props
}) => {
  const currentPath = useLocation().pathname
  const navigate = useNavigate()

  return (
    <SwipeableDrawer anchor="left" open={open} onClose={onClose} onOpen={onOpen} {...props}>
      <Box sx={{ width: 250 }} role="presentation" onClick={onClose} onKeyDown={onClose}>
        <List>
          {pages.map((page) => (
            <AuthenticatedNavListItem
              key={page.name}
              currentPath={currentPath}
              navigate={navigate}
              page={page}
              basePath={basePath}
              disabled={
                page.requireAuth && page.permissionCallback
                  ? !page.permissionCallback(guildPermissions)
                  : false
              }
            />
          ))}
        </List>
        {children}
      </Box>
    </SwipeableDrawer>
  )
}
