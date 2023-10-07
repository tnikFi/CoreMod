import { ListItem, ListItemButton, ListItemIcon, ListItemText, Tooltip } from '@mui/material'
import React from 'react'

interface GuildProps {
  onClick?: () => void
  name: string
  icon?: string | null
}

const Guild: React.FC<GuildProps> = ({ onClick, name, icon }) => {
  return (
    <ListItem disablePadding>
      <Tooltip title={name} placement="right">
        <ListItemButton onClick={onClick}>
          <ListItemIcon sx={{ width: 40, height: 40 }}>
            {icon ? (
              <img src={icon ?? undefined} style={{ borderRadius: '50%' }} alt={name} />
            ) : (
              <div
                style={{ width: 40, height: 40, backgroundColor: '#888', borderRadius: '50%' }}
                aria-label={name}
              >
                <div style={{ color: '#ffffff', textAlign: 'center', marginTop: 7 }} aria-hidden>
                  {name[0]}
                </div>
              </div>
            )}
          </ListItemIcon>
          <ListItemText primary={name} primaryTypographyProps={{ noWrap: true }} />
        </ListItemButton>
      </Tooltip>
    </ListItem>
  )
}

export default Guild
