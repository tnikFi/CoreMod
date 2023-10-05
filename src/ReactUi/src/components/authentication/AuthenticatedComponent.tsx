import React from 'react'
import { AuthContext } from 'react-oauth2-code-pkce'

const AuthenticatedComponent: React.FC<React.PropsWithChildren> = ({ children }) => {
  const { token } = React.useContext(AuthContext)

  if (!token) return null

  return <>{children}</>
}

export default AuthenticatedComponent
