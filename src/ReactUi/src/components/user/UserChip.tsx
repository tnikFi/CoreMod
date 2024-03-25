import { Avatar, Chip, ChipProps, Tooltip, alpha } from '@mui/material'
import React from 'react'
import styles from './UserChip.module.css'
import UserContextMenu from './UserContextMenu'

export interface UserChipProps extends Omit<ChipProps, 'color'> {
  userId: string
  username?: string
  nickname?: string | null
  color?: string | null
  avatarUrl?: string | null
  loading?: boolean
}

interface ContextMenuPosition {
  mouseX: number
  mouseY: number
}

const UserChip: React.FC<UserChipProps> = ({
  userId,
  username,
  nickname,
  color,
  avatarUrl,
  loading,
  ...props
}) => {
  const [contextMenu, setContextMenu] = React.useState<ContextMenuPosition | null>(null)

  const handleContextMenu = (event: React.MouseEvent<HTMLDivElement>) => {
    event.preventDefault()
    setContextMenu(
      contextMenu
        ? null
        : {
            mouseX: event.clientX - 2,
            mouseY: event.clientY - 4,
          }
    )
  }

  return (
    <span onContextMenu={handleContextMenu}>
      <Tooltip title={username || userId} placement="bottom">
        <Chip
          component="div"
          avatar={
            <Avatar alt={username} src={avatarUrl || undefined}>
              {!loading && username ? (nickname ? nickname[0] : username[0]) : null}
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
          {...props}
        />
      </Tooltip>
      <UserContextMenu
        open={contextMenu !== null}
        onClose={() => setContextMenu(null)}
        anchorReference='anchorPosition'
        anchorPosition={
          contextMenu
            ? { top: contextMenu.mouseY, left: contextMenu.mouseX }
            : undefined
        }
        userId={userId}
        username={username}
        nickname={nickname}
        avatarUrl={avatarUrl}
      />
    </span>
  )
}

export default UserChip
