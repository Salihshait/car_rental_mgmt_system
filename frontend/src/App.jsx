import { Routes, Route } from 'react-router-dom';
import { Box } from '@mui/material';
import DashboardPage from './pages/DashboardPage';
import AuthPage from './pages/AuthPage';
import BookingPage from './pages/BookingPage';

export default function App() {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f7fb' }}>
      <Routes>
        <Route path="/" element={<AuthPage />} />
        <Route path="/login" element={<AuthPage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/booking" element={<BookingPage />} />
      </Routes>
    </Box>
  );
}
