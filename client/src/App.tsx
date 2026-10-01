import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { StorefrontPage } from './pages/StorefrontPage'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StorefrontPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
