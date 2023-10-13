import React from 'react'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import PageView from '../../components/PageView'
import { GuildsService, ModerationDto, RoleDto, UserService } from '../../api'
import MyModerations from '../../components/panel/MyModerations'
import GuildStats from '../../components/panel/GuildStats'
import { Grid } from '@mui/material'

const Overview = () => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)
  const [moderations, setModerations] = React.useState<ModerationDto[]>([])
  const [moderationsLoading, setModerationsLoading] = React.useState(false)
  const [userRoles, setUserRoles] = React.useState<RoleDto[]>([])
  const [guildMemberCount, setGuildMemberCount] = React.useState(0)
  const [guildStatsLoading, setGuildStatsLoading] = React.useState(false)

  /**
   * Fetches the user's moderations for the selected guild.
   */
  const getModerations = React.useCallback(async () => {
    setModerations([])
    if (!selectedGuild?.id) return
    setModerationsLoading(true)
    try {
      const moderations = await UserService.getApiUserModerations(selectedGuild.id)
      setModerations(moderations)
    } finally {
      setModerationsLoading(false)
    }
  }, [selectedGuild])

  /**
   * Fetches the member count and roles for the selected guild.
   */
  const updateGuildStats = React.useCallback(async () => {
    setGuildMemberCount(0)
    setUserRoles([])
    if (!selectedGuild?.id) return
    setGuildStatsLoading(true)
    try {
      const count = await GuildsService.getApiGuildsMemberCount(selectedGuild.id)
      const roles = await UserService.getApiUserRoles(selectedGuild.id)
      setGuildMemberCount(count)
      setUserRoles(roles)
    } finally {
      setGuildStatsLoading(false)
    }
  }, [selectedGuild])

  React.useEffect(() => {
    getModerations()
    updateGuildStats()
  }, [updateGuildStats, getModerations])

  if (!selectedGuild) return null

  return (
    <PageView>
      <Grid container spacing={2} columns={{ xs: 1, xl: 3 }}>
        <Grid
          item
          xs={1}
          sx={{
            height: { lg: '100%', xl: 'auto' },
          }}
        >
          <GuildStats
            guildMemberCount={guildMemberCount}
            roles={userRoles}
            selectedGuild={selectedGuild}
            loading={guildStatsLoading}
          />
        </Grid>
        <Grid item xs={1} xl={2}>
          <MyModerations
            moderations={moderations}
            loading={moderationsLoading}
            onRefresh={getModerations}
          />
        </Grid>
      </Grid>
    </PageView>
  )
}

export default Overview
