import { GridValueFormatterParams } from '@mui/x-data-grid'

/**
 * Formats a date time string to a human readable format using the system locale and timezone.
 * The date string is expected to be in UTC.
 */
export const dateTimeFormatter = (params: GridValueFormatterParams) => {
  const date = new Date(params.value as string)
  return date.toLocaleString()
}

/**
 * Formats an expiration date time string to a human readable format using the system locale and timezone.
 * If the expiration date time is undefined, returns 'Never'.
 */
export const expirationTimeFormatter = (params: GridValueFormatterParams) => {
  return params.value ? dateTimeFormatter(params) : 'Never'
}
