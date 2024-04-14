import { Chip, CircularProgress, IconButton } from '@mui/material'
import React from 'react'
import CircleIcon from '@mui/icons-material/Circle'
import { alpha } from '@mui/material'
import { RoleDto } from '../../api'
import CloseIcon from '@mui/icons-material/Close'

interface RoleChipProps extends RoleDto {
  /**
   * Whether the role can be deleted by the user.
   */
  canDelete?: boolean

  /**
   * Callback that is called when the role is deleted.
   */
  onDelete?: () => void

  /**
   * Whether to display a loading spinner and disable the delete button.
   */
  loading?: boolean
}

const RoleChip: React.FC<RoleChipProps> = ({ name, color, canDelete, onDelete, loading }) => {
  const [isHovered, setIsHovered] = React.useState(false)

  return (
    <Chip
      icon={
        loading ? (
          <CircularProgress size={24} sx={{ 'svg circle': { stroke: color } }} />
        ) : (
        canDelete ? (
          <IconButton sx={{ p: 0 }}>
            {isHovered ? (
              <CloseIcon sx={{ height: 24, fill: color }} onClick={onDelete} />
            ) : (
              <CircleIcon sx={{ height: 24, fill: color }} />
            )}
          </IconButton>
        ) : (
          <CircleIcon sx={{ height: 24, fill: color }} />
        ))
      }
      label={name}
      variant="outlined"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onFocus={() => setIsHovered(true)}
      onBlur={() => setIsHovered(false)}
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
    />
  )
}

export default RoleChip
