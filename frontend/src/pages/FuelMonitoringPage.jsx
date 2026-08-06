import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
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
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { createFuelLog, getFuelConsumptionSummary, listFuelLogs, listVehicles } from '../services/api';

const emptyForm = { vehicleId: '', quantity: '', cost: '', odometerReading: '', logType: 'Refuel' };

export default function FuelMonitoringPage() {
  const [vehicles, setVehicles] = useState([]);
  const [vehicleId, setVehicleId] = useState('');
  const [logs, setLogs] = useState([]);
  const [summary, setSummary] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [message, setMessage] = useState('');

  useEffect(() => {
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    loadLogs();
  }, []);

  const loadLogs = (id) => {
    listFuelLogs(id).then(setLogs).catch((error) => setMessage(error.message));
  };

  const handleVehicleFilter = (value) => {
    setVehicleId(value);
    loadLogs(value || undefined);
    if (value) {
      getFuelConsumptionSummary(value).then(setSummary).catch((error) => setMessage(error.message));
    } else {
      setSummary(null);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await createFuelLog({
        vehicleId: form.vehicleId,
        quantity: Number(form.quantity),
        cost: Number(form.cost),
        odometerReading: form.odometerReading ? Number(form.odometerReading) : null,
        logType: form.logType,
      });
      setForm(emptyForm);
      loadLogs(vehicleId || undefined);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const chartData = [...logs]
    .sort((a, b) => new Date(a.loggedOn) - new Date(b.loggedOn))
    .map((log) => ({ date: new Date(log.loggedOn).toLocaleDateString(), cost: log.cost, quantity: log.quantity }));

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Fuel Monitoring</Typography>

        <Card>
          <CardContent>
            <Stack component="form" onSubmit={handleSubmit} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField select label="Vehicle" required value={form.vehicleId} onChange={(e) => setForm({ ...form, vehicleId: e.target.value })} sx={{ minWidth: 200 }}>
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
                ))}
              </TextField>
              <TextField select label="Type" value={form.logType} onChange={(e) => setForm({ ...form, logType: e.target.value })} sx={{ minWidth: 140 }}>
                <MenuItem value="Refuel">Refuel</MenuItem>
                <MenuItem value="Consumption">Consumption</MenuItem>
              </TextField>
              <TextField label="Quantity" type="number" required value={form.quantity} onChange={(e) => setForm({ ...form, quantity: e.target.value })} sx={{ width: 120 }} />
              <TextField label="Cost" type="number" required value={form.cost} onChange={(e) => setForm({ ...form, cost: e.target.value })} sx={{ width: 120 }} />
              <TextField label="Odometer" type="number" value={form.odometerReading} onChange={(e) => setForm({ ...form, odometerReading: e.target.value })} sx={{ width: 130 }} />
              <Button type="submit" variant="contained">Log Fuel</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <TextField select label="Filter by vehicle" value={vehicleId} onChange={(e) => handleVehicleFilter(e.target.value)} sx={{ minWidth: 240 }}>
              <MenuItem value="">All vehicles</MenuItem>
              {vehicles.map((vehicle) => (
                <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
              ))}
            </TextField>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        {summary ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Total Quantity</Typography><Typography variant="h5">{summary.totalQuantity}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Total Cost</Typography><Typography variant="h5">{summary.totalCost}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Log Count</Typography><Typography variant="h5">{summary.logCount}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Distance / Unit</Typography><Typography variant="h5">{summary.distancePerUnit ?? '-'}</Typography></CardContent></Card>
            </Grid>
          </Grid>
        ) : null}

        {chartData.length > 0 ? (
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Cost Over Time</Typography>
              <Box sx={{ height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="cost" stroke="#0d6efd" />
                  </LineChart>
                </ResponsiveContainer>
              </Box>
            </CardContent>
          </Card>
        ) : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Vehicle</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Quantity</TableCell>
                    <TableCell>Cost</TableCell>
                    <TableCell>Odometer</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {logs.map((log) => (
                    <TableRow key={log.id} hover>
                      <TableCell>{log.vehicleRegistrationNumber}</TableCell>
                      <TableCell>{new Date(log.loggedOn).toLocaleString()}</TableCell>
                      <TableCell>{log.logType}</TableCell>
                      <TableCell>{log.quantity}</TableCell>
                      <TableCell>{log.cost}</TableCell>
                      <TableCell>{log.odometerReading ?? '-'}</TableCell>
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
