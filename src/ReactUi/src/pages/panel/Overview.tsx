import React from 'react'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import PageView from '../../components/PageView'
import { Box, Button, Container, Paper, Typography } from '@mui/material'
import { ModerationDto, UserService } from '../../api'
import { DataGrid, GridColDef, GridValueFormatterParams } from '@mui/x-data-grid'

/**
 * Formats a date time string to a human readable format using the system locale and timezone.
 * The date string is expected to be in UTC.
 */
const dateTimeFormatter = (params: GridValueFormatterParams) => {
  // Parse the date and apply the system timezone offset
  const date = new Date(params.value as string)
  date.setMinutes(date.getMinutes() - date.getTimezoneOffset())
  return date.toLocaleString()
}

const moderationColumns: GridColDef[] = [
  { field: 'createdAt', headerName: 'Created At', width: 200, valueFormatter: dateTimeFormatter },
  { field: 'type', headerName: 'Type', width: 200 },
  { field: 'reason', headerName: 'Reason', minWidth: 200, flex: 1 },
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
      <Container>
        <Paper sx={{ p: 2, height: 500, display: 'flex', flexDirection: 'column' }}>
          <Box
            sx={{
              marginBottom: 2,
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            <Typography variant="h6">My Moderations</Typography>
            <Button variant="outlined" disabled={moderationsLoading} onClick={getModerations}>
              Refresh
            </Button>
          </Box>
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
