import { Box, TextField, TextFieldProps, Typography } from '@mui/material'
import React from 'react'

type LimitedTextFieldProps = TextFieldProps & {
  maxLength?: number
}

const LimitedTextField: React.FC<LimitedTextFieldProps> = ({ maxLength, ...props }) => {
  const [textLength, setTextLength] = React.useState<number>(0)

  React.useEffect(() => {
    setTextLength(props.value?.toString().length || 0)
  }, [props.value])

  return (
    <Box>
      <TextField {...props} inputProps={{ maxLength }}></TextField>
      {maxLength && (
        <Box sx={{ textAlign: 'right', color: 'text.secondary' }}>
          <Typography variant="caption">
            <Typography
              variant="caption"
              sx={{ fontWeight: 'bold' }}
              color={textLength >= maxLength ? 'error' : 'inherit'}
            >
              {textLength}
            </Typography>{' '}
            / {maxLength}
          </Typography>
        </Box>
      )}
    </Box>
  )
}

export default LimitedTextField
