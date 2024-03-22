import { Avatar, Chip, alpha } from '@mui/material'
import React from 'react'
import styles from './UserChip.module.css'

export interface UserChipProps {
  userId: string
  username?: string
  nickname?: string | null
  color?: string | null
  avatarUrl?: string | null
  loading?: boolean
}

const UserChip: React.FC<UserChipProps> = ({ userId, username, nickname, color, avatarUrl, loading }) => {
  return (
    <Chip
      component="div" // Add the missing component prop with the value of 'div'
      avatar={
        <Avatar alt={username} src={avatarUrl || undefined}>
          {!loading && username ? nickname ? nickname[0] : username[0] : null}
        </Avatar>
      }
      label={nickname || username || userId}
      variant="outlined"
      disabled={loading}
      sx={{
        p: 0.5,
        width: 'fit-content',
        height: 'fit-content',
        borderColor: color,
        boxShadow: (theme) => theme.shadows[1],
        ':hover': {
          backgroundColor: color ? alpha(color, 0.1) : undefined,
        },
      }}
      className={loading ? styles.loading : ''}
    />
  )
}

export default UserChip
