import { Paper, Box, Avatar, Typography, SxProps, Theme, CircularProgress } from '@mui/material'
import React from 'react'
import RolesContainer from '../roles/RolesContainer'
import { GuildDto, RoleDto } from '../../api'

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
  loading,
  sx,
}) => {
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
        <Typography variant="h6" mb={2}>
          My Roles
        </Typography>
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
    </Paper>
  )
}

export default GuildStats
