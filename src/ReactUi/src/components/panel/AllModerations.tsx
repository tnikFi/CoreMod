import React from 'react'
import { ApiError, GuildsService, ModerationDto } from '../../api'
import { getModerationRowClassName, isExpired } from '../../utils/ModerationGridUtils'
import { Box, Button, Paper, Typography, styled } from '@mui/material'
import { DataGrid, GridColDef, GridEventListener, gridClasses } from '@mui/x-data-grid'
import { dateTimeFormatter, expirationTimeFormatter } from '../../utils/ValueFormatters'
import CheckIcon from '@mui/icons-material/Check'
import ServerSideGrid, {
  ServerSideDataSourceParams,
  ServerSideGridRef,
} from '../grid/ServerSideGrid'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import { UserCellRenderer } from '../../utils/CellRenderers'
import { disableColumnSorting } from '../../utils/GridUtils'
import ModerationEditor, { ModerationEditorRef } from './ModerationEditor'
import { useSnackbar } from 'notistack'

/**
 * Columns for the moderation data grid.
 */
const moderationColumns: GridColDef[] = disableColumnSorting([
  { field: 'id', headerName: 'Case ID', width: 75 },
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
])

/**
 * Data grid for displaying the user's moderations.
 * Expired moderations are displayed in a different color.
 */
const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  [`& .${gridClasses.row}.expired`]: {
    color: theme.palette.text.secondary,
  },
}))

const AllModerations: React.FC = () => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)
  const gridRef = React.useRef<ServerSideGridRef | null>(null)
  const editorRef = React.useRef<ModerationEditorRef | null>(null)
  const { enqueueSnackbar } = useSnackbar()

  const moderationsDataSource = React.useCallback(
    async (params: ServerSideDataSourceParams) => {
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
    },
    [selectedGuild?.id]
  )

  const handleRefresh = React.useCallback(() => {
    gridRef.current?.reset()
  }, [])

  const handleGridRowDoubleClick: GridEventListener<'rowDoubleClick'> = React.useCallback(
    (params) => {
      editorRef.current?.editModeration(params.row as ModerationDto)
      console.log(editorRef.current)
    },
    [editorRef]
  )

  const handleChangesSaved = React.useCallback(
    (newModeration: ModerationDto) => {
      gridRef.current?.dataGridRef.current?.updateRows([newModeration])
      enqueueSnackbar('Moderation changes saved.', { variant: 'success' })
    },
    [enqueueSnackbar, gridRef]
  )

  const handleSaveFailed = React.useCallback(
    (error: ApiError) => {
      enqueueSnackbar(`Failed to save moderation changes: ${error.message}`, {
        variant: 'error',
      })
    },
    [enqueueSnackbar]
  )

  const handleModerationDeleted = React.useCallback(
    () => {
      gridRef.current?.loadRows()
      enqueueSnackbar('Moderation deleted.', { variant: 'success' })
    },
    [enqueueSnackbar, gridRef]
  )

  const handleModerationDeleteFailed = React.useCallback(
    (error: ApiError) => {
      enqueueSnackbar(`Failed to delete moderation: ${error.message}`, {
        variant: 'error',
      })
    },
    [enqueueSnackbar]
  )

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
        <Button variant="outlined" disabled={gridRef.current?.loading} onClick={handleRefresh}>
          Refresh
        </Button>
      </Box>
      <ServerSideGrid
        component={StyledDataGrid}
        columns={moderationColumns}
        dataSource={moderationsDataSource}
        getRowClassName={getModerationRowClassName}
        rowBuffer={100}
        ref={gridRef}
        disableColumnFilter
        onRowDoubleClick={handleGridRowDoubleClick}
      />
      <ModerationEditor
        onChangesSaved={handleChangesSaved}
        onSaveFailed={handleSaveFailed}
        onDeleted={handleModerationDeleted}
        onDeleteFailed={handleModerationDeleteFailed}
        ref={editorRef}
      />
    </Paper>
  )
}

export default AllModerations
