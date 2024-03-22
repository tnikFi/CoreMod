import { Menu, MenuItem, MenuProps } from '@mui/material'
import React from 'react'

interface UserContextMenuProps extends MenuProps {
  userId: string
  username?: string
  nickname?: string | null
  avatarUrl?: string | null
  open: boolean
}

const UserContextMenu: React.FC<UserContextMenuProps> = ({ userId, username, nickname, avatarUrl, open, onClose, ...props }) => {
  const copyOptionalString = (str?: string | null) => {
    if (str) navigator.clipboard.writeText(str)
    if (onClose) onClose({}, 'backdropClick')
  }

  const getMenuItemCallback = (str?: string | null) => () => copyOptionalString(str)
  
  return (
    <Menu open={open} onClose={onClose} {...props}>
      <MenuItem onClick={getMenuItemCallback(username)} disabled={!username}>Copy Username</MenuItem>
      <MenuItem onClick={getMenuItemCallback(nickname)} disabled={!nickname}>Copy Nickname</MenuItem>
      <MenuItem onClick={getMenuItemCallback(avatarUrl)} disabled={!avatarUrl}>Copy avatar link</MenuItem>
      <MenuItem onClick={getMenuItemCallback(userId)}>Copy User ID</MenuItem>
    </Menu>
  )
}

export default UserContextMenu