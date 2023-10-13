import { Paper, Box, Typography, Button } from '@mui/material'
import { DataGrid, GridColDef, GridValueFormatterParams } from '@mui/x-data-grid'
import React from 'react'
import { ModerationDto } from '../../api'

export interface MyModerationsProps {
  /**
   * Moderations to display in the data grid.
   */
  moderations: ModerationDto[]

  /**
   * Whether the moderations are currently being loaded. Disables the refresh button.
   * @default false
   */
  loading: boolean

  /**
   * Callback to refresh the moderations. Called when the refresh button is clicked.
   */
  onRefresh: () => void
}

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

/**
 * Columns for the moderation data grid.
 */
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

const MyModerations: React.FC<MyModerationsProps> = ({
  moderations,
  loading: moderationsLoading,
  onRefresh: getModerations,
}) => {
  return (
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
  )
}

export default MyModerations
