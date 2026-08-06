import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
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
import { assignDriver, createDriver, listDriverAssignments, listDrivers, listUsers, listVehicles, unassignDriver } from '../services/api';

export default function DriverAssignmentsPage() {
  const [drivers, setDrivers] = useState([]);
  const [users, setUsers] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [driverForm, setDriverForm] = useState({ userId: '', licenseNumber: '' });
  const [assignForm, setAssignForm] = useState({ driverId: '', vehicleId: '' });
  const [message, setMessage] = useState('');

  const loadDrivers = () => {
    listDrivers().then(setDrivers).catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listUsers().then(setUsers).catch((error) => setMessage(error.message));
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    loadDrivers();
  }, []);

  const handleCreateDriver = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await createDriver(driverForm);
      setDriverForm({ userId: '', licenseNumber: '' });
      loadDrivers();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAssign = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await assignDriver(assignForm);
      setAssignForm({ driverId: '', vehicleId: '' });
      loadDrivers();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleUnassign = async (driver) => {
    setMessage('');
    try {
      const history = await listDriverAssignments({ driverId: driver.id });
      const active = history.find((a) => !a.unassignedAt);
      if (active) {
        await unassignDriver(active.id);
        loadDrivers();
      }
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Driver Assignments</Typography>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Register Driver</Typography>
            <Stack component="form" onSubmit={handleCreateDriver} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="User" required value={driverForm.userId} onChange={(e) => setDriverForm({ ...driverForm, userId: e.target.value })} sx={{ minWidth: 240 }}>
                {users.map((user) => (
                  <MenuItem key={user.id} value={user.id}>{user.firstName} {user.lastName} ({user.email})</MenuItem>
                ))}
              </TextField>
              <TextField label="License Number" required value={driverForm.licenseNumber} onChange={(e) => setDriverForm({ ...driverForm, licenseNumber: e.target.value })} sx={{ minWidth: 180 }} />
              <Button type="submit" variant="contained">Add Driver</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Assign Driver to Vehicle</Typography>
            <Stack component="form" onSubmit={handleAssign} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="Driver" required value={assignForm.driverId} onChange={(e) => setAssignForm({ ...assignForm, driverId: e.target.value })} sx={{ minWidth: 200 }}>
                {drivers.map((driver) => (
                  <MenuItem key={driver.id} value={driver.id}>{driver.name}</MenuItem>
                ))}
              </TextField>
              <TextField select label="Vehicle" required value={assignForm.vehicleId} onChange={(e) => setAssignForm({ ...assignForm, vehicleId: e.target.value })} sx={{ minWidth: 200 }}>
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
                ))}
              </TextField>
              <Button type="submit" variant="contained">Assign</Button>
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
                    <TableCell>Driver</TableCell>
                    <TableCell>License</TableCell>
                    <TableCell>Current Vehicle</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {drivers.map((driver) => (
                    <TableRow key={driver.id} hover>
                      <TableCell>{driver.name}</TableCell>
                      <TableCell>{driver.licenseNumber}</TableCell>
                      <TableCell>{driver.currentVehicleRegistrationNumber ?? '-'}</TableCell>
                      <TableCell>
                        {driver.currentVehicleId ? (
                          <Button size="small" color="error" onClick={() => handleUnassign(driver)}>Unassign</Button>
                        ) : null}
                      </TableCell>
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
