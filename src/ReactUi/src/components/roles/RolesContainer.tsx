import React from 'react'
import RoleChip from './RoleChip'
import { Box, SxProps, Theme } from '@mui/material'
import { RoleDto } from '../../api'

export interface RolesContainerProps {
  roles: RoleDto[]
  deletableRoles?: RoleDto[]
  onRoleDeleted?: (role: RoleDto) => void
  
  /**
   * Show a loading spinner and disable the delete button for all roles.
   */
  loading?: boolean
  sx?: SxProps<Theme>
}

const RolesContainer: React.FC<RolesContainerProps> = ({
  roles,
  deletableRoles,
  onRoleDeleted,
  loading,
  sx,
}) => {
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
      {roles.map((role) => (
        <RoleChip
          key={role.id}
          canDelete={deletableRoles?.some((x) => x.id === role.id)}
          onDelete={onRoleDeleted ? () => onRoleDeleted(role) : undefined}
          loading={loading}
          {...role}
        />
      ))}
    </Box>
  )
}

export default RolesContainer
