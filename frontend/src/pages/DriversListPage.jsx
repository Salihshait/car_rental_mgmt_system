import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { createDriver, getDriverDashboard, listDrivers, listUsers } from '../services/api';

const licenseColor = (status) => {
  switch (status) {
    case 'Valid':
      return 'success';
    case 'ExpiringSoon':
      return 'warning';
    case 'Expired':
      return 'error';
    default:
      return 'default';
  }
};

export default function DriversListPage() {
  const [drivers, setDrivers] = useState([]);
  const [users, setUsers] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [form, setForm] = useState({ userId: '', licenseNumber: '' });
  const [message, setMessage] = useState('');

  const loadDrivers = () => listDrivers().then(setDrivers).catch((error) => setMessage(error.message));

  useEffect(() => {
    listUsers().then(setUsers).catch((error) => setMessage(error.message));
    getDriverDashboard().then(setDashboard).catch((error) => setMessage(error.message));
    loadDrivers();
  }, []);

  const handleCreate = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await createDriver(form);
      setForm({ userId: '', licenseNumber: '' });
      loadDrivers();
      getDriverDashboard().then(setDashboard).catch(() => {});
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Driver Roster</Typography>

        {dashboard ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Total Drivers</Typography><Typography variant="h5">{dashboard.totalDrivers}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Active</Typography><Typography variant="h5">{dashboard.activeDrivers}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">On Leave Today</Typography><Typography variant="h5">{dashboard.onLeaveToday}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Average Rating</Typography><Typography variant="h5">{dashboard.averageRating ?? '-'}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Licenses Expiring Soon</Typography><Typography variant="h5">{dashboard.licensesExpiringSoonCount}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Licenses Expired</Typography><Typography variant="h5">{dashboard.licensesExpiredCount}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Trips This Month</Typography><Typography variant="h5">{dashboard.totalTripsThisMonth}</Typography></CardContent></Card>
            </Grid>
          </Grid>
        ) : null}

        <Card>
          <CardContent>
            <Stack component="form" onSubmit={handleCreate} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="User" required value={form.userId} onChange={(e) => setForm({ ...form, userId: e.target.value })} sx={{ minWidth: 240 }}>
                {users.map((user) => (
                  <MenuItem key={user.id} value={user.id}>{user.firstName} {user.lastName} ({user.email})</MenuItem>
                ))}
              </TextField>
              <TextField label="License Number" required value={form.licenseNumber} onChange={(e) => setForm({ ...form, licenseNumber: e.target.value })} sx={{ minWidth: 180 }} />
              <Button type="submit" variant="contained">Register Driver</Button>
            </Stack>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>License</TableCell>
                    <TableCell>License Status</TableCell>
                    <TableCell>Employment</TableCell>
                    <TableCell>Current Vehicle</TableCell>
                    <TableCell>Rating</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {drivers.map((driver) => (
                    <TableRow key={driver.id} hover>
                      <TableCell><RouterLink to={`/drivers/${driver.id}`}>{driver.name}</RouterLink></TableCell>
                      <TableCell>{driver.licenseNumber}</TableCell>
                      <TableCell><Chip label={driver.licenseStatus} color={licenseColor(driver.licenseStatus)} size="small" /></TableCell>
                      <TableCell>{driver.employmentStatus}</TableCell>
                      <TableCell>{driver.currentVehicleRegistrationNumber ?? '-'}</TableCell>
                      <TableCell>{driver.rating ?? '-'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
