import { Chip } from '@mui/material'
import React from 'react'
import CircleIcon from '@mui/icons-material/Circle'
import { alpha } from '@mui/material'
import { RoleDto } from '../../api'

const RoleChip: React.FC<RoleDto> = ({ name, color }) => {
  return (
    <Chip
      icon={<CircleIcon sx={{ height: 15, fill: color }} />}
      label={name}
      variant="outlined"
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
