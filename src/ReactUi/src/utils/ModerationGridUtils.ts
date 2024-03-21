import { GridRowClassNameParams } from '@mui/x-data-grid'

/**
 * Checks whether the expiration date time has passed.
 * @param expiresAt Expiration date time string.
 * @returns Boolean indicating whether the expiration date time has passed. Returns false if the expiration date time is undefined.
 */
export const isExpired = (expiresAt: string | undefined) => {
  if (!expiresAt) return false
  return new Date(expiresAt) < new Date()
}

/**
 * Returns the class name for a row in a moderation data grid based on the expiration date time.
 */
export const getModerationRowClassName = (params: GridRowClassNameParams) => {
  if (isExpired(params.row.expiresAt as string | undefined)) {
    return 'expired'
  }
  return ''
}
