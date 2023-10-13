import React from 'react'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import PageView from '../../components/PageView'
import { ModerationDto, UserService } from '../../api'
import MyModerations from '../../components/panel/MyModerations'

const Overview = () => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)
  const [moderations, setModerations] = React.useState<ModerationDto[]>([])
  const [moderationsLoading, setModerationsLoading] = React.useState(false)

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

  React.useEffect(() => {
    getModerations()
  }, [getModerations])

  if (!selectedGuild) return null

  return (
    <PageView>
      <MyModerations
        moderations={moderations}
        loading={moderationsLoading}
        onRefresh={getModerations}
      />
    </PageView>
  )
}

export default Overview
