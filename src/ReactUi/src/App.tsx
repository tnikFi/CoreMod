import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './pages/Layout'
import Home from './pages/Home'
import NotFound from './pages/NotFound'
import Panel from './pages/panel/Panel'
import Overview from './pages/panel/Overview'
import { CssBaseline } from '@mui/material'

const App = () => {
  return (
    <BrowserRouter>
      <CssBaseline />
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />
          <Route path="*" element={<NotFound />} />
        </Route>
        <Route path="panel" element={<Panel />}>
          <Route path="" element={<Overview />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
