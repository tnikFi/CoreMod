import React from 'react'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import PageView from '../PageView'
import { Box, Button, CircularProgress, Modal, Typography } from '@mui/material'
import { GuildPermissionCallback } from '../../utils/PermissionCallbacks'

interface RequireGuildPermissionsProps {
  /**
   * A function that returns a boolean indicating whether the user has the required permissions.
   *
   * Actions performed in the restricted page should still be validated on the server to prevent unauthorized access.
   * @param permissions Permissions the user has in the current guild. `undefined` if no guild is selected or if permissions have not been fetched.
   * @returns Boolean indicating if the user has permission to view the page.
   */
  permissionCallback: GuildPermissionCallback
}

const modalStyle = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  bgcolor: 'background.paper',
  boxShadow: 24,
  p: 4,
}

const RequireGuildPermissions: React.FC<React.PropsWithChildren<RequireGuildPermissionsProps>> = ({
  permissionCallback,
  children,
}) => {
  const { guildPermissions } = React.useContext(SelectedGuildContext)

  return (
    <>
      <Modal open={guildPermissions === undefined}>
        <Box sx={modalStyle}>
          <CircularProgress sx={{ mb: 2 }}/>
          <Typography variant="h6">Fetching permissions...</Typography>
        </Box>
      </Modal>
      {
        /* show a loading indicator while permissions are being fetched */
        guildPermissions === undefined ? null : permissionCallback(guildPermissions) ? (
          children
        ) : (
          <PageView>
            <h1>Forbidden</h1>
            <p style={{ fontSize: 24 }}>
              You do not have permission to view this page for the current guild.
            </p>
            <Button onClick={() => window.history.back()}>Go Back</Button>
          </PageView>
        )
      }
    </>
  )
}

export default RequireGuildPermissions
