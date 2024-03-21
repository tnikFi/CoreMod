import { Paper, Box, Typography, Button, styled } from '@mui/material'
import { DataGrid, GridColDef, gridClasses } from '@mui/x-data-grid'
import React from 'react'
import { ModerationDto } from '../../api'
import CheckIcon from '@mui/icons-material/Check'
import { valueFormatters } from '../../utils/ValueFormatters'
import { getModerationRowClassName, isExpired } from '../../utils/ModerationGridUtils'

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
 * Columns for the moderation data grid.
 */
const moderationColumns: GridColDef[] = [
  {
    field: 'createdAt',
    headerName: 'Created At',
    width: 200,
    valueFormatter: valueFormatters.dateTimeFormatter,
  },
  { field: 'type', headerName: 'Type', width: 200 },
  { field: 'reason', headerName: 'Reason', minWidth: 200, flex: 1 },
  {
    field: 'expiresAt',
    headerName: 'Expires At',
    width: 200,
    valueFormatter: valueFormatters.expirationTimeFormatter,
  },
  {
    field: 'expired',
    headerName: 'Expired',
    width: 100,
    valueGetter: (params) => {
      const expiresAt = params.row.expiresAt as string | undefined
      if (!expiresAt) return undefined
      return params.value ? true : expiresAt ? isExpired(expiresAt) : false
    },
    renderCell: (params) => {
      return params.value ? <CheckIcon /> : undefined
    },
  },
]

/**
 * Data grid for displaying the user's moderations.
 * Expired moderations are displayed in a different color.
 */
const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  [`& .${gridClasses.row}.expired`]: {
    color: theme.palette.text.secondary,
  },
}))

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
      <StyledDataGrid
        columns={moderationColumns}
        rows={moderations}
        autoHeight={false}
        loading={moderationsLoading}
        getRowClassName={getModerationRowClassName}
      />
    </Paper>
  )
}

export default MyModerations
