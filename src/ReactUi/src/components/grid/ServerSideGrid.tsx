import React from 'react'
import { DataGrid, DataGridProps, GridPaginationModel } from '@mui/x-data-grid'

interface RowData {
  [key: string]: unknown
}

interface ServerSideGridData {
  rows: RowData[]
  totalRows: number
}

export interface ServerSideDataSourceParams {
  paginationModel: GridPaginationModel
  success: (data: ServerSideGridData) => void
  failure: (error: Error) => void
}

interface ServerSideGridProps extends Omit<DataGridProps, 'rows'> {
  /**
   * Optional component to use as the root of the grid.
   * The component should have the same props as the `DataGrid` component.
   */
  component?: React.ComponentType<DataGridProps>

  /**
   * Function to fetch the data to display in the grid.
   * @param paginationModel Current pagination model state of the grid.
   * @returns An object containing the rows to display and the total number of rows.
   */
  dataSource: (params: ServerSideDataSourceParams) => Promise<void> | void

  /**
   * Optional callback to execute when an error occurs during the data fetch.
   * @param error Error object that occurred during the data fetch.
   * @returns
   */
  onError?: (error: Error) => void

  /**
   * Whether the grid should cache the data it fetches to speed up navigation through previously fetched pages.
   */
  useCache?: boolean
}

export interface ServerSideGridRef {
  /**
   * Invalidates the cache of the grid, forcing it to fetch new data next time rows are loaded.
   */
  invalidateCache: () => void

  /**
   * Loads the rows of the grid using the current pagination model.
   */
  loadRows: () => void

  /**
   * Clears the rows of the grid.
   */
  clearRows: (preserveCache?: boolean) => void

  /**
   * Resets the grid to its initial state, causing cache to be invalidated, rows to be cleared, and the initial rows to be loaded again.
   */
  reset: () => void

  /**
   * The data grid ref.
   */
  dataGridRef: React.RefObject<HTMLDivElement>

  /**
   * Whether the grid is currently loading data.
   */
  loading: boolean
}

const ServerSideGrid = React.forwardRef<ServerSideGridRef, ServerSideGridProps>(
  ({ dataSource, component, useCache, onError, ...props }, ref) => {
    const [rowCache, setRowCache] = React.useState<ServerSideGridData>({ rows: [], totalRows: 0 })
    const [rows, setRows] = React.useState<RowData[]>([])
    const [loading, setLoading] = React.useState<boolean>(false)
    const [paginationModel, setPaginationModel] = React.useState<GridPaginationModel>({
      pageSize: 25,
      page: 0,
    })
    const [rowCountState, setRowCountState] = React.useState<number>(0)

    const invalidateCache = React.useCallback(() => {
      setRowCache({ rows: [], totalRows: 0 })
    }, [])

    const fetchSuccess = React.useCallback(
      (data: ServerSideGridData) => {
        setRows(data.rows)
        setRowCountState(data.totalRows)
        console.log(data.totalRows)
      },
      [setRows, setRowCountState]
    )

    const fetchFailure = React.useCallback(
      (error: Error) => {
        if (onError) {
          onError(error)
        } else {
          console.error('Failed to fetch data for ServerSideGrid', error)
        }
      },
      [onError]
    )

    const loadRows = React.useCallback(
      async (paginationModel: GridPaginationModel) => {
        setLoading(true)
        // Calculate the start index of the data to fetch.
        const cacheIndex = paginationModel.page * paginationModel.pageSize

        // If the cache is enabled and all the data is already cached, use the cache.
        // Only use the cache if the total number of rows hasn't changed since the cache was created.
        if (
          useCache &&
          rowCache.rows[cacheIndex] &&
          rowCache.rows.length >= cacheIndex + paginationModel.pageSize &&
          rowCache.totalRows === rowCountState
        ) {
          setRows(rowCache.rows.slice(cacheIndex, cacheIndex + paginationModel.pageSize))
          setRowCountState(rowCache.totalRows)
          setLoading(false)
          return
        }

        // Invalidate the cache if the total number of rows has changed.
        if (rowCache.totalRows !== rowCountState) {
          invalidateCache()
        }

        const dataSourceParams = {
          paginationModel,
          success: (data: ServerSideGridData) => {
            fetchSuccess(data)
            if (useCache) {
              setRowCache({
                rows: [
                  ...rowCache.rows.slice(0, cacheIndex),
                  ...data.rows,
                  ...rowCache.rows.slice(cacheIndex + paginationModel.pageSize),
                ],
                totalRows: data.totalRows,
              })
            }
          },
          failure: fetchFailure,
        }

        await dataSource(dataSourceParams)

        setLoading(false)
        console.log('Rows loaded')
      },
      [dataSource, fetchFailure, fetchSuccess, invalidateCache, rowCache, rowCountState, useCache]
    )

    // Expose the ref methods.
    const dataGridRef = React.useRef<HTMLDivElement>(null)
    React.useImperativeHandle(
      ref,
      () => ({
        invalidateCache,
        loadRows: () => loadRows(paginationModel),
        clearRows: (preserveCache = false) => {
          setRows([])
          setRowCountState(0)
          if (!preserveCache) {
            invalidateCache()
          }
        },
        reset: () => {
          invalidateCache()
          setRows([])
          setRowCountState(0)
          setPaginationModel({ pageSize: 25, page: 0 })
          loadRows({ pageSize: 25, page: 0 })
        },
        dataGridRef: dataGridRef,
        loading,
      }),
      [invalidateCache, loadRows, loading, paginationModel]
    )

    const GridComponent = component ?? DataGrid

    // Load the initial rows.
    React.useEffect(() => {
      const loadInitialRows = async () => {
        await loadRows(paginationModel)
      }
      loadInitialRows()
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])
    // Dependency array for this MUST be empty to prevent infinite loops.

    return (
      <GridComponent
        rows={rows}
        rowCount={rowCountState}
        loading={props.loading ?? loading}
        paginationMode="server"
        paginationModel={paginationModel}
        onPaginationModelChange={(model) => {
          setPaginationModel(model)
          loadRows(model)
        }}
        ref={dataGridRef}
        {...props}
      />
    )
  }
)

export default ServerSideGrid
