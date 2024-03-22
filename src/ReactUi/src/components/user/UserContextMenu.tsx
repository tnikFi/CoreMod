import { Menu, MenuItem } from '@mui/material'
import React from 'react'

interface UserContextMenuProps {
  open: boolean
  handleClose: () => void
  userId: string
}

const UserContextMenu: React.FC<UserContextMenuProps> = ({ open, handleClose, userId }) => {
  const copyUserId = () => {
    navigator.clipboard.writeText(userId)
    handleClose()
  }
  
  return (
    <Menu open={open} onClose={handleClose}>
      <MenuItem onClick={copyUserId}>Copy User ID</MenuItem>
    </Menu>
  )
}

export default UserContextMenu