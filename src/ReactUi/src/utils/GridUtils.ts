import { GridColDef } from "@mui/x-data-grid"

export const disableColumnSorting = (columns: GridColDef[]): GridColDef[] => {
  return columns.map((column) => {
    return {
      ...column,
      sortable: false,
    }
  })
}
