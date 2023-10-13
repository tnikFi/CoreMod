import React from 'react'
import RoleChip from './RoleChip'
import { Box, SxProps, Theme } from '@mui/material'
import { RoleDto } from '../../api'

export interface RolesContainerProps {
  roles: RoleDto[]
  sx?: SxProps<Theme>
}

const RolesContainer: React.FC<RolesContainerProps> = ({ roles, sx }) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexFlow: 'row',
        flexWrap: 'wrap',
        alignItems: 'center',
        width: '100%',
        gap: 1,
        ...sx,
      }}
    >
      {roles.map((role, index) => (
        <RoleChip key={index} {...role} />
      ))}
    </Box>
  )
}

export default RolesContainer
