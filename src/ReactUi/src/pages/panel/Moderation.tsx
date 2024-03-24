import RequireGuildPermissions from '../../components/authorization/RequireGuildPermissions'
import { permissionCallbacks } from '../../utils/PermissionCallbacks'
import PageView from '../../components/PageView'
import { Grid } from '@mui/material'
import AllModerations from '../../components/panel/AllModerations'

const Moderation = () => {
  return (
    <RequireGuildPermissions permissionCallback={permissionCallbacks.viewModerationPage}>
      <PageView>
        <Grid container spacing={2} columns={{ xs: 1, xl: 3 }}>
          <Grid
            item
            xs={1}
            xl={3}
            sx={{
              height: { lg: '100%', xl: 'auto' },
            }}
          >
            <AllModerations />
          </Grid>
        </Grid>
      </PageView>
    </RequireGuildPermissions>
  )
}

export default Moderation
