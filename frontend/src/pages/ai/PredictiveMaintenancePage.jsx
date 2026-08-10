import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
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
import { generateMaintenancePredictions, listMaintenancePredictions, updateMaintenancePredictionStatus } from '../../services/api';

const statusColor = (status) => {
  switch (status) {
    case 'Open': return 'warning';
    case 'Scheduled': return 'info';
    case 'Dismissed': return 'default';
    default: return 'default';
  }
};

export default function PredictiveMaintenancePage() {
  const [predictions, setPredictions] = useState([]);
  const [statusFilter, setStatusFilter] = useState('Open');
  const [error, setError] = useState('');
  const [generating, setGenerating] = useState(false);

  const load = () => {
    listMaintenancePredictions({ status: statusFilter }).then(setPredictions).catch((err) => setError(err.message));
  };

  useEffect(load, [statusFilter]);

  const handleGenerate = async () => {
    setGenerating(true);
    try {
      await generateMaintenancePredictions();
      load();
    } catch (err) {
      setError(err.message);
    } finally {
      setGenerating(false);
    }
  };

  const handleStatusChange = async (id, status) => {
    await updateMaintenancePredictionStatus(id, status);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Predictive Maintenance</Typography>
          <Button variant="contained" onClick={handleGenerate} disabled={generating}>Generate Predictions</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} sx={{ minWidth: 160 }}>
          <MenuItem value="">All</MenuItem>
          {['Open', 'Scheduled', 'Dismissed'].map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </TextField>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Vehicle</TableCell>
                <TableCell>Predicted Issue</TableCell>
                <TableCell>Due Date</TableCell>
                <TableCell>Confidence</TableCell>
                <TableCell>Basis</TableCell>
                <TableCell>Status</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {predictions.map((p) => (
                <TableRow key={p.id} hover>
                  <TableCell>{p.vehicleRegistrationNumber ?? '-'}</TableCell>
                  <TableCell>{p.predictedIssue}</TableCell>
                  <TableCell>{new Date(p.predictedDueDate).toLocaleDateString()}</TableCell>
                  <TableCell>{p.confidenceScore}%</TableCell>
                  <TableCell sx={{ maxWidth: 320 }}>{p.basisSummary}</TableCell>
                  <TableCell><Chip size="small" label={p.status} color={statusColor(p.status)} /></TableCell>
                  <TableCell>
                    {p.status === 'Open' ? (
                      <Stack direction="row" spacing={0.5}>
                        <Button size="small" onClick={() => handleStatusChange(p.id, 'Scheduled')}>Mark Scheduled</Button>
                        <Button size="small" color="error" onClick={() => handleStatusChange(p.id, 'Dismissed')}>Dismiss</Button>
                      </Stack>
                    ) : null}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>
    </Box>
  );
}
