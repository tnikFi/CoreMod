import React from 'react'
import { ApiError, GuildsService, ModerationDto } from '../../api'
import { getModerationRowClassName, isExpired } from '../../utils/ModerationGridUtils'
import { Box, Button, Paper, Typography, styled } from '@mui/material'
import { DataGrid, GridColDef, gridClasses } from '@mui/x-data-grid'
import { dateTimeFormatter, expirationTimeFormatter } from '../../utils/ValueFormatters'
import CheckIcon from '@mui/icons-material/Check'
import ServerSideGrid, { ServerSideDataSourceParams } from '../grid/ServerSideGrid'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import { UserCellRenderer } from '../../utils/CellRenderers'

export interface AllModerationsProps {
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
  { field: 'userId', headerName: 'User', width: 200, renderCell: UserCellRenderer },
  { field: 'type', headerName: 'Type', width: 125 },
  {
    field: 'createdAt',
    headerName: 'Created At',
    width: 200,
    valueFormatter: dateTimeFormatter,
  },
  { field: 'moderatorId', headerName: 'Moderator', width: 200, renderCell: UserCellRenderer },
  { field: 'reason', headerName: 'Reason', minWidth: 200, flex: 1 },
  {
    field: 'expiresAt',
    headerName: 'Expires At',
    width: 200,
    valueFormatter: expirationTimeFormatter,
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
      return params.value ? <CheckIcon /> : null
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

const AllModerations: React.FC<AllModerationsProps> = ({ loading, onRefresh }) => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)

  const moderationsDataSource = React.useCallback(async (params: ServerSideDataSourceParams) => {
    try {
      const data = await GuildsService.getApiGuildsModerations(
        selectedGuild?.id ?? '',
        params.paginationModel.page,
        params.paginationModel.pageSize
      )
      params.success({ rows: data.data ?? [], totalRows: data.totalItems ?? 0 })
    } catch (error) {
      params.failure(error as ApiError)
    }
  }, [selectedGuild?.id])

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
        <Typography variant="h6">Moderations</Typography>
        <Button variant="outlined" disabled={loading} onClick={onRefresh}>
          Refresh
        </Button>
      </Box>
      <ServerSideGrid
        component={StyledDataGrid}
        columns={moderationColumns}
        dataSource={moderationsDataSource}
        useCache={false}
        getRowClassName={getModerationRowClassName}
        rowBuffer={100}
      />
    </Paper>
  )
}

export default AllModerations
