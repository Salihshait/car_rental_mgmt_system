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
  cancelVehicleTransfer,
  completeVehicleTransfer,
  createVehicleTransfer,
  listBranches,
  listVehicleTransfers,
  listVehicles,
} from '../services/api';

const statusColor = (status) => {
  switch (status) {
    case 'InTransit':
      return 'warning';
    case 'Completed':
      return 'success';
    case 'Cancelled':
      return 'default';
    default:
      return 'default';
  }
};

export default function VehicleTransfersPage() {
  const [vehicles, setVehicles] = useState([]);
  const [branches, setBranches] = useState([]);
  const [transfers, setTransfers] = useState([]);
  const [vehicleId, setVehicleId] = useState('');
  const [toBranchId, setToBranchId] = useState('');
  const [notes, setNotes] = useState('');
  const [message, setMessage] = useState('');

  const loadTransfers = () => {
    listVehicleTransfers().then(setTransfers).catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    listBranches().then(setBranches).catch((error) => setMessage(error.message));
    loadTransfers();
  }, []);

  const selectedVehicle = vehicles.find((v) => v.id === vehicleId);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await createVehicleTransfer({ vehicleId, toBranchId, notes: notes || null });
      setVehicleId('');
      setToBranchId('');
      setNotes('');
      loadTransfers();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAction = async (action, id) => {
    setMessage('');
    try {
      if (action === 'complete') await completeVehicleTransfer(id);
      if (action === 'cancel') await cancelVehicleTransfer(id);
      loadTransfers();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Vehicle Transfers</Typography>

        <Card>
          <CardContent>
            <Stack component="form" onSubmit={handleSubmit} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="Vehicle" required value={vehicleId} onChange={(e) => setVehicleId(e.target.value)} sx={{ minWidth: 200 }}>
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
                ))}
              </TextField>
              <TextField label="From Branch" value={selectedVehicle?.branchName ?? ''} InputProps={{ readOnly: true }} sx={{ minWidth: 180 }} />
              <TextField select label="To Branch" required value={toBranchId} onChange={(e) => setToBranchId(e.target.value)} sx={{ minWidth: 180 }}>
                {branches.map((branch) => (
                  <MenuItem key={branch.id} value={branch.id}>{branch.name}</MenuItem>
                ))}
              </TextField>
              <TextField label="Notes" value={notes} onChange={(e) => setNotes(e.target.value)} sx={{ minWidth: 200 }} />
              <Button type="submit" variant="contained">Request Transfer</Button>
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
                    <TableCell>Vehicle</TableCell>
                    <TableCell>From</TableCell>
                    <TableCell>To</TableCell>
                    <TableCell>Requested</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transfers.map((transfer) => (
                    <TableRow key={transfer.id} hover>
                      <TableCell>{transfer.vehicleRegistrationNumber}</TableCell>
                      <TableCell>{transfer.fromBranchName}</TableCell>
                      <TableCell>{transfer.toBranchName}</TableCell>
                      <TableCell>{new Date(transfer.requestedAt).toLocaleString()}</TableCell>
                      <TableCell><Chip label={transfer.status} color={statusColor(transfer.status)} size="small" /></TableCell>
                      <TableCell>
                        {transfer.status === 'InTransit' ? (
                          <Stack direction="row" spacing={1}>
                            <Button size="small" onClick={() => handleAction('complete', transfer.id)}>Complete</Button>
                            <Button size="small" color="error" onClick={() => handleAction('cancel', transfer.id)}>Cancel</Button>
                          </Stack>
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
