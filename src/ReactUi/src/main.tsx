import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import { AuthProvider, TAuthConfig } from 'react-oauth2-code-pkce'

const authConfig: TAuthConfig = {
  clientId: import.meta.env.VITE_CLIENT_ID as string,
  authorizationEndpoint: import.meta.env.VITE_AUTHORIZATION_ENDPOINT as string,
  tokenEndpoint: import.meta.env.VITE_TOKEN_ENDPOINT as string,
  logoutEndpoint: import.meta.env.VITE_LOGOUT_ENDPOINT as string,
  redirectUri: import.meta.env.VITE_REDIRECT_URI as string,
  decodeToken: false,
  autoLogin: false,
  scope: 'identify',
  clearURL: true,
  preLogin: () => {
    sessionStorage.setItem('redirect', window.location.pathname)
  },
  postLogin: () => {
    const redirect = sessionStorage.getItem('redirect')
    if (redirect) {
      window.location.replace(redirect)
    }
    sessionStorage.removeItem('redirect')
  },
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider authConfig={authConfig}>
      <App />
    </AuthProvider>
  </React.StrictMode>
)
