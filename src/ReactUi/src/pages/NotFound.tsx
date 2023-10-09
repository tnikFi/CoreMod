import React from 'react'
import { Box, Button, Typography } from '@mui/material'

const messages = [
  'The page you are looking for is in another castle.',
  'This is not the page you are looking for.',
  'How did you get here?',
  'Nothing to see here.',
  'Page not found.',
  'Move along, nothing to see here.',
  'Not all those who wander are lost.',
  'If you got here by using the site normally, please raise an issue on GitHub.',
  'Keep looking, you will find it eventually.',
]

const NotFound = () => {
  React.useEffect(() => {
    document.title = 'Not Found'
  }, [])

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'column',
        height: '60vh',
      }}
    >
      <Typography variant="h1" color="GrayText">
        404
      </Typography>
      <Typography variant="h5" sx={{ my: 2 }}>
        {messages[Math.floor(Math.random() * messages.length)]}
      </Typography>
      <Button onClick={() => window.history.back()}>Go Back</Button>
    </Box>
  )
}

export default NotFound
