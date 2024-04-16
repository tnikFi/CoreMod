import {
  Box,
  Button,
  Container,
  Divider,
  Grid,
  Link,
  Paper,
  Typography,
  useTheme,
} from '@mui/material'
import React from 'react'
import AuthenticatedComponent from '../components/authentication/AuthenticatedComponent'
import UnauthenticatedComponent from '../components/authentication/UnauthenticatedComponent'
import { useNavigate } from 'react-router-dom'
import { AuthContext } from 'react-oauth2-code-pkce'
import GitHubIcon from '@mui/icons-material/GitHub'
import BugReportIcon from '@mui/icons-material/BugReport'
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch'
import MenuBookIcon from '@mui/icons-material/MenuBook'

const Home = () => {
  const theme = useTheme()
  const navigate = useNavigate()
  const { login } = React.useContext(AuthContext)
  const isDarkMode = React.useMemo(() => theme.palette.mode === 'dark', [theme.palette.mode])

  return (
    <>
      <Container sx={{ display: 'flex', flexDirection: 'column', gap: 10, mb: 10 }}>
        <Box height={'60vh'} display={'flex'} flexDirection={'column'} justifyContent={'center'}>
          <Box
            mb={2}
            display={'flex'}
            justifyContent={'space-between'}
            gap={3}
            textAlign={{ xs: 'center', md: 'left' }}
          >
            <Box display={'flex'} flexDirection={'column'} gap={3}>
              <Typography variant="h3">Moderation, administration, open source.</Typography>
              <Typography variant="subtitle1">
                An open source Discord bot that provides moderation and administration tools for
                your server. Invite the bot to your server to get started.
              </Typography>
            </Box>
            <Paper sx={{ display: { xs: 'none', md: 'initial' }, height: 200 }} elevation={4}>
              {isDarkMode ? (
                <img
                  src="/images/panel_overview_desktop.png"
                  alt="Panel Overview"
                  height={'100%'}
                />
              ) : (
                <img
                  src="/images/panel_overview_desktop_light.png"
                  alt="Panel Overview"
                  height={'100%'}
                />
              )}
            </Paper>
          </Box>
          <Box display={'flex'} gap={2} justifyContent={{ xs: 'center', md: 'start' }}>
            <Button
              variant="contained"
              color="primary"
              href={import.meta.env.VITE_INVITE_URL}
              target="_blank"
            >
              Invite to Server
            </Button>
            <UnauthenticatedComponent>
              <Button variant="outlined" color="primary" onClick={() => login()}>
                Login with Discord
              </Button>
            </UnauthenticatedComponent>
            <AuthenticatedComponent>
              <Button variant="outlined" color="primary" onClick={() => navigate('/panel')}>
                Open Control Panel
              </Button>
            </AuthenticatedComponent>
          </Box>
        </Box>

        <Divider />

        <section>
          <Typography variant="h4" gutterBottom>
            Features
          </Typography>
          <Grid container spacing={2} columns={{ xs: 1, md: 2 }} rowSpacing={4}>
            <Grid item xs={1}>
              <Typography variant="h5">Moderation</Typography>
              <Typography>
                Moderation commands to keep your server safe and clean. Kick, ban, mute and warn
                users to enforce your server's rules.
              </Typography>
            </Grid>
            <Grid item xs={1}>
              <Typography variant="h5">Utilities</Typography>
              <Typography>
                Customize public self-assigned roles, set up user reporting, and more to enhance the
                user experience on your server.
              </Typography>
            </Grid>
            <Grid item xs={1}>
              <Typography variant="h5">Customizable</Typography>
              <Typography>
                Configure the bot's settings to fit your server's needs. Choose the channels used
                for logging and reporting, set up a welcome message, or customize the bot's prefix.
              </Typography>
            </Grid>
            <Grid item xs={1}>
              <Typography variant="h5" gutterBottom>
                Extensible
              </Typography>
              <Typography>
                The bot is built with extensibility in mind using{' '}
                <Link
                  href="https://docs.discordnet.dev/"
                  aria-label="Read Discord.Net documentation"
                >
                  Discord.Net
                </Link>
                . If you're a developer, you can create your own commands and features or modify
                existing ones to fit your requirements.
              </Typography>
            </Grid>
          </Grid>
        </section>

        <Divider />

        <section>
          <Typography variant="h4" gutterBottom>
            About the Project
          </Typography>
          <Grid container columns={{ xs: 1, md: 2 }} columnSpacing={8} rowSpacing={4}>
            <Grid item xs={1}>
              <Typography variant="h5" gutterBottom>
                Open Source
              </Typography>
              <Typography mb={1}>
                The bot is open source and available on GitHub. If you spot a bug or find yourself
                in need of a feature that the bot doesn't provide, feel free to submit a bug report
                or feature request.
              </Typography>
            </Grid>
            <Grid item xs={1}>
              <Typography variant="h5" gutterBottom>
                Cross-Platform Support
              </Typography>
              <Typography>
                The bot is built using .NET 7 with cross-platform support in mind. If you want to
                self-host the bot, you can do so on the platform of your choice.
              </Typography>
            </Grid>
          </Grid>
        </section>
      </Container>

      <Box sx={{ backgroundColor: theme.palette.divider }} py={5}>
        <Container>
          <Typography variant="h6" color={theme.palette.text.secondary} gutterBottom>
            Project Links
          </Typography>
          <div>
            <Button
              href={import.meta.env.VITE_GITHUB_URL}
              target="_blank"
              startIcon={<GitHubIcon />}
              variant="text"
              sx={{ color: theme.palette.text.secondary }}
            >
              GitHub repository
            </Button>
          </div>
          <div>
            <Button
              href={import.meta.env.VITE_BUG_REPORT_URL}
              target="_blank"
              startIcon={<BugReportIcon />}
              variant="text"
              sx={{ color: theme.palette.error.main }}
            >
              Report a bug
            </Button>
          </div>
          <div>
            <Button
              href={import.meta.env.VITE_FEATURE_REQUEST_URL}
              target="_blank"
              startIcon={<RocketLaunchIcon />}
              variant="text"
              sx={{ color: theme.palette.text.secondary }}
            >
              Request a feature
            </Button>
          </div>
          <div>
            <Button
              href={import.meta.env.VITE_DOCUMENTATION_URL}
              target="_blank"
              startIcon={<MenuBookIcon />}
              variant="text"
              sx={{ color: theme.palette.text.secondary }}
            >
              Documentation
            </Button>
          </div>
        </Container>
      </Box>
    </>
  )
}

export default Home
