import { AppBar, Box, Button, Stack, Toolbar, Typography } from '@mui/material';
import { Outlet, useNavigate, Link as RouterLink } from 'react-router-dom';
import { supabase } from '../services/supabaseClient';

const navItems = [
  { label: 'Dashboard', to: '/dashboard' },
  { label: 'Bookings', to: '/booking' },
  { label: 'Billing', to: '/invoices' },
  { label: 'Vehicles', to: '/vehicles' },
  { label: 'Vehicle Catalog', to: '/vehicle-catalog' },
  { label: 'Customers', to: '/customers' },
  { label: 'Users', to: '/users' },
  { label: 'Roles', to: '/roles' },
  { label: 'Permissions', to: '/permissions' },
  { label: 'My Account', to: '/my-account' },
  { label: 'Notifications', to: '/notifications' },
  { label: 'Profile', to: '/profile' },
];

export default function AppShell() {
  const navigate = useNavigate();

  const handleLogout = async () => {
    await supabase.auth.signOut();
    navigate('/login');
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f7fb' }}>
      <AppBar position="static" color="default" elevation={1}>
        <Toolbar sx={{ gap: 1, flexWrap: 'wrap' }}>
          <Typography variant="h6" sx={{ mr: 3 }}>Car Rent</Typography>
          <Stack direction="row" spacing={1} sx={{ flexGrow: 1, flexWrap: 'wrap' }}>
            {navItems.map((item) => (
              <Button key={item.to} component={RouterLink} to={item.to} size="small">
                {item.label}
              </Button>
            ))}
          </Stack>
          <Button size="small" onClick={handleLogout}>Logout</Button>
        </Toolbar>
      </AppBar>
      <Outlet />
    </Box>
  );
}
