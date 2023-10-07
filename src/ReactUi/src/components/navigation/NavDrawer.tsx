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

export interface Page {
  name: string
  path: string
  requireAuth?: boolean
  icon?: React.ReactNode
}

interface NavMenuProps extends SwipeableDrawerProps {
  pages: Page[]
  open: boolean
  onClose: () => void
  onOpen: () => void
}

interface NavListItemProps {
  page: Page
  currentPath: string
  navigate: (path: string) => void
}

/**
 * A nav list item that is always rendered.
 */
const NavListItem: React.FC<NavListItemProps> = ({ currentPath, navigate, page }) => {
  return (
    <ListItem key={page.name} disablePadding>
      <ListItemButton
        onClick={() => navigate(page.path)}
        selected={'/panel' + page.path === currentPath || '/panel/' + page.path === currentPath}
      >
        <ListItemIcon>{page.icon}</ListItemIcon>
        <ListItemText primary={page.name} />
      </ListItemButton>
    </ListItem>
  )
}

/**
 * A nav list item that is only rendered if the user is authenticated if the page requires authentication.
 */
const AuthenticatedNavListItem: React.FC<NavListItemProps> = ({ currentPath, navigate, page }) => {
  return page.requireAuth ? (
    <AuthenticatedComponent>
      <NavListItem currentPath={currentPath} navigate={navigate} page={page} />
    </AuthenticatedComponent>
  ) : (
    <NavListItem currentPath={currentPath} navigate={navigate} page={page} />
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
            />
          ))}
        </List>
        {children}
      </Box>
    </SwipeableDrawer>
  )
}
