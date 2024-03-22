import { GridRenderCellParams } from '@mui/x-data-grid'
import LazyUserChip from '../components/user/LazyUserChip'

export const UserCellRenderer = (params: GridRenderCellParams) => {
  const userId = params.value as string | undefined
  if (!userId) return null
  return (<LazyUserChip userId={userId} defaultLabel={userId} />)
}
