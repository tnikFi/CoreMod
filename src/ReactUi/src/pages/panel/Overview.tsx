import React from 'react'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import PageView from '../../components/PageView'
import { Container, Paper, Typography } from '@mui/material'
import { ModerationDto, UserService } from '../../api'
import { DataGrid, GridColDef, GridValueFormatterParams } from '@mui/x-data-grid'

/**
 * Formats a date time string to a human readable format using the system locale.
 */
const dateTimeFormatter = (params: GridValueFormatterParams) => {
  const date = new Date(params.value)
  return date.toLocaleString()
}

const moderationColumns: GridColDef[] = [
  { field: 'createdAt', headerName: 'Created At', width: 200, valueFormatter: dateTimeFormatter },
  { field: 'type', headerName: 'Type', width: 200 },
  { field: 'reason', headerName: 'Reason', width: 200 },
  {
    field: 'expiresAt',
    headerName: 'Expires At',
    width: 200,
    valueFormatter: (params) => (params.value ? dateTimeFormatter(params) : 'Never'),
  },
]

const Overview = () => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)
  const [moderations, setModerations] = React.useState<ModerationDto[]>([])
  const [moderationsLoading, setModerationsLoading] = React.useState(false)

  React.useEffect(() => {
    const getModerations = async () => {
      setModerations([])
      if (!selectedGuild?.id) return
      setModerationsLoading(true)
      try {
        const moderations = await UserService.getApiUserModerations(selectedGuild.id)
        setModerations(moderations)
      } finally {
        setModerationsLoading(false)
      }
    }
    getModerations()
  }, [selectedGuild])

  if (!selectedGuild) return null

  return (
    <PageView>
      <Container>
        <Paper sx={{ p: 2, maxWidth: '100%' }}>
          <Typography variant="h6" sx={{ marginBottom: 2 }}>
            My Moderations
          </Typography>
          <DataGrid
            columns={moderationColumns}
            rows={moderations}
            autoHeight={true}
            loading={moderationsLoading}
          />
        </Paper>
      </Container>
    </PageView>
  )
}

export default Overview
