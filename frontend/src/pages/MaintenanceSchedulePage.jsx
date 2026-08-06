import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
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
import {
  cancelMaintenance,
  completeMaintenance,
  listMaintenance,
  listVehicles,
  scheduleMaintenance,
  startMaintenance,
} from '../services/api';

const emptyForm = { vehicleId: '', serviceType: '', scheduledOn: '', cost: '', notes: '' };

const statusColor = (status) => {
  switch (status) {
    case 'Scheduled':
      return 'info';
    case 'InProgress':
      return 'warning';
    case 'Completed':
      return 'success';
    case 'Cancelled':
      return 'default';
    default:
      return 'default';
  }
};

export default function MaintenanceSchedulePage() {
  const [vehicles, setVehicles] = useState([]);
  const [records, setRecords] = useState([]);
  const [status, setStatus] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [message, setMessage] = useState('');

  const loadRecords = (filters = {}) => {
    listMaintenance(filters).then(setRecords).catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    loadRecords();
  }, []);

  const handleStatusFilter = (value) => {
    setStatus(value);
    loadRecords(value ? { status: value } : {});
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await scheduleMaintenance({
        vehicleId: form.vehicleId,
        serviceType: form.serviceType,
        scheduledOn: form.scheduledOn,
        cost: form.cost ? Number(form.cost) : null,
        notes: form.notes || null,
      });
      setForm(emptyForm);
      loadRecords(status ? { status } : {});
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAction = async (action, id) => {
    setMessage('');
    try {
      if (action === 'start') await startMaintenance(id);
      if (action === 'complete') await completeMaintenance(id, {});
      if (action === 'cancel') await cancelMaintenance(id);
      loadRecords(status ? { status } : {});
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Maintenance Schedule</Typography>

        <Card>
          <CardContent>
            <Stack component="form" onSubmit={handleSubmit} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="Vehicle" required value={form.vehicleId} onChange={(e) => setForm({ ...form, vehicleId: e.target.value })} sx={{ minWidth: 200 }}>
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
                ))}
              </TextField>
              <TextField label="Service Type" required value={form.serviceType} onChange={(e) => setForm({ ...form, serviceType: e.target.value })} sx={{ minWidth: 180 }} />
              <TextField
                label="Scheduled On"
                type="datetime-local"
                required
                InputLabelProps={{ shrink: true }}
                value={form.scheduledOn}
                onChange={(e) => setForm({ ...form, scheduledOn: e.target.value })}
                sx={{ minWidth: 220 }}
              />
              <TextField label="Estimated Cost" type="number" value={form.cost} onChange={(e) => setForm({ ...form, cost: e.target.value })} sx={{ width: 140 }} />
              <TextField label="Notes" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} sx={{ minWidth: 200 }} />
              <Button type="submit" variant="contained">Schedule</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <TextField select label="Status" value={status} onChange={(e) => handleStatusFilter(e.target.value)} sx={{ minWidth: 180 }}>
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Scheduled">Scheduled</MenuItem>
              <MenuItem value="InProgress">In Progress</MenuItem>
              <MenuItem value="Completed">Completed</MenuItem>
              <MenuItem value="Cancelled">Cancelled</MenuItem>
            </TextField>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Vehicle</TableCell>
                    <TableCell>Service</TableCell>
                    <TableCell>Scheduled On</TableCell>
                    <TableCell>Cost</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {records.map((record) => (
                    <TableRow key={record.id} hover>
                      <TableCell>{record.vehicleRegistrationNumber}</TableCell>
                      <TableCell>{record.serviceType}</TableCell>
                      <TableCell>{new Date(record.scheduledOn).toLocaleString()}</TableCell>
                      <TableCell>{record.cost ?? '-'}</TableCell>
                      <TableCell><Chip label={record.status} color={statusColor(record.status)} size="small" /></TableCell>
                      <TableCell>
                        <Stack direction="row" spacing={1}>
                          {record.status === 'Scheduled' ? (
                            <Button size="small" onClick={() => handleAction('start', record.id)}>Start</Button>
                          ) : null}
                          {record.status === 'InProgress' ? (
                            <Button size="small" onClick={() => handleAction('complete', record.id)}>Complete</Button>
                          ) : null}
                          {record.status !== 'Completed' && record.status !== 'Cancelled' ? (
                            <Button size="small" color="error" onClick={() => handleAction('cancel', record.id)}>Cancel</Button>
                          ) : null}
                        </Stack>
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
