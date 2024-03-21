import React from 'react'
import { AuthContext } from 'react-oauth2-code-pkce'
import AuthenticatedComponent from './AuthenticatedComponent'
import UnauthenticatedComponent from './UnauthenticatedComponent'
import PageView from '../PageView'
import { Button } from '@mui/material'

const RequireAuthenticated: React.FC<React.PropsWithChildren> = ({ children }) => {
  const { login } = React.useContext(AuthContext)

  return (
    <>
      <AuthenticatedComponent>{children}</AuthenticatedComponent>
      <UnauthenticatedComponent>
        <PageView>
          <h1>Unauthorized</h1>
          <p style={{ fontSize: 24 }}>You are not logged in.</p>
          <Button onClick={() => login()}>Login</Button>
        </PageView>
      </UnauthenticatedComponent>
    </>
  )
}

export default RequireAuthenticated
