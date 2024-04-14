import {
  Paper,
  Box,
  Avatar,
  Typography,
  SxProps,
  Theme,
  CircularProgress,
  Tooltip,
  IconButton,
} from '@mui/material'
import React from 'react'
import RolesContainer from '../roles/RolesContainer'
import { ApiError, GuildDto, RoleDto, UserService } from '../../api'
import AddIcon from '@mui/icons-material/Add'
import AddPublicRoleModal from '../modals/AddPublicRoleModal'
import { useSnackbar } from 'notistack'

export interface GuildStatsProps {
  /**
   * The number of members in the selected guild.
   */
  guildMemberCount: number

  /**
   * Roles of the user in the selected guild.
   */
  roles: RoleDto[]

  /**
   * The selected guild.
   */
  selectedGuild: GuildDto

  /**
   * Public roles of the selected guild.
   */
  publicRoles?: RoleDto[]

  /**
   * Callback that is called when the user roles are changed.
   * @param roles New roles of the user in the selected guild.
   * @returns
   */
  onRolesChanged?: (roles: RoleDto[]) => void

  loading?: boolean

  /**
   * Additional styles for the paper root element.
   */
  sx?: SxProps<Theme>
}

const GuildStats: React.FC<GuildStatsProps> = ({
  guildMemberCount,
  roles,
  selectedGuild,
  publicRoles,
  onRolesChanged,
  loading,
  sx,
}) => {
  const [roleSelectionOpen, setRoleSelectionOpen] = React.useState(false)
  const { enqueueSnackbar } = useSnackbar()

  const handleAddPublicRole = async (role: RoleDto) => {
    try {
      // Get the public roles the user has
      const userPublicRoles = roles.filter((r) => publicRoles?.some((pr) => pr.id === r.id))

      // Check if the user already has the role
      if (userPublicRoles.some((r) => r.id === role.id)) return

      // Add the role to the user
      await UserService.patchApiUserPublicRoles(
        selectedGuild.id ?? undefined,
        userPublicRoles.concat(role)
      )
      if (onRolesChanged) onRolesChanged(roles.concat(role))
      enqueueSnackbar(`Role "${role.name}" added`, { variant: 'success' })
    } catch (error) {
      enqueueSnackbar(`Failed to add role: ${(error as ApiError).message}`, { variant: 'error' })
      console.error(error)
    }
  }

  return (
    <Paper sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column', ...sx }}>
      <Box
        sx={{
          marginBottom: 2,
          display: 'flex',
          justifyContent: 'start',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <Avatar src={selectedGuild?.icon ?? undefined} alt={selectedGuild?.name ?? undefined}>
          {selectedGuild?.name ? selectedGuild.name[0] : undefined}
        </Avatar>
        <Box>
          <Typography variant="h6">{selectedGuild.name}</Typography>
          <Typography variant="body2" color="GrayText">
            {selectedGuild?.id}
          </Typography>
        </Box>
      </Box>
      <Box
        sx={{
          marginBottom: 2,
          display: 'flex',
          justifyContent: 'start',
          alignItems: 'baseline',
        }}
        aria-label="Member count"
      >
        <Typography
          variant="h3"
          component={'div'}
          color={'GrayText'}
          sx={{ opacity: 0.5 }}
          textAlign={'start'}
          aria-hidden
        >
          {guildMemberCount.toString().padStart(6, '0').match(/^0+/)}
        </Typography>
        {guildMemberCount > 0 && (
          <Typography variant="h3" component={'div'} textAlign={'start'}>
            {guildMemberCount}
          </Typography>
        )}
        <Typography variant="body1" color="GrayText" ml={2}>
          Members
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Box display={'flex'} justifyContent={'space-between'} alignItems={'center'}>
          <Typography variant="h6" mb={2}>
            My Roles
          </Typography>
          <Tooltip title="Add public roles">
            <span>
              <IconButton
                aria-label="Add public roles"
                disabled={!publicRoles || publicRoles.length === 0}
                onClick={() => setRoleSelectionOpen(true)}
              >
                <AddIcon />
              </IconButton>
            </span>
          </Tooltip>
        </Box>
        <Box flexGrow={0} height={{ xs: 140, xl: 240 }} overflow="auto">
          {loading ? (
            <CircularProgress size={32} />
          ) : roles.length > 0 ? (
            <RolesContainer roles={roles} />
          ) : (
            <Typography
              variant="body1"
              color="GrayText"
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100%',
              }}
            >
              No roles
            </Typography>
          )}
        </Box>
      </Paper>

      <AddPublicRoleModal
        open={roleSelectionOpen && publicRoles !== undefined && publicRoles.length > 0}
        roles={publicRoles ? publicRoles.filter((r) => !roles.some((ur) => ur.id === r.id)) : []}
        onRoleAdded={handleAddPublicRole}
        onClose={() => setRoleSelectionOpen(false)}
      />
    </Paper>
  )
}

export default GuildStats
