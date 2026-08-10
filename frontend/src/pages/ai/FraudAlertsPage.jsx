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
import { listFraudAlerts, reviewFraudAlert } from '../../services/api';

const statusColor = (status) => {
  switch (status) {
    case 'Open': return 'warning';
    case 'Confirmed': return 'error';
    case 'Reviewed': return 'info';
    case 'Dismissed': return 'default';
    default: return 'default';
  }
};

const riskColor = (score) => (score >= 70 ? 'error' : score >= 40 ? 'warning' : 'default');

export default function FraudAlertsPage() {
  const [alerts, setAlerts] = useState([]);
  const [statusFilter, setStatusFilter] = useState('Open');
  const [error, setError] = useState('');

  const load = () => {
    listFraudAlerts({ status: statusFilter }).then(setAlerts).catch((err) => setError(err.message));
  };

  useEffect(load, [statusFilter]);

  const handleReview = async (id, status) => {
    await reviewFraudAlert(id, status);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Fraud Alerts</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} sx={{ minWidth: 160 }}>
          <MenuItem value="">All</MenuItem>
          {['Open', 'Reviewed', 'Dismissed', 'Confirmed'].map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </TextField>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Customer</TableCell>
                <TableCell>Risk Score</TableCell>
                <TableCell>Reasons</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {alerts.map((a) => (
                <TableRow key={a.id} hover>
                  <TableCell>{a.customerName ?? '-'}</TableCell>
                  <TableCell><Chip size="small" label={a.riskScore} color={riskColor(a.riskScore)} /></TableCell>
                  <TableCell sx={{ maxWidth: 360 }}>{a.reasons}</TableCell>
                  <TableCell><Chip size="small" label={a.status} color={statusColor(a.status)} /></TableCell>
                  <TableCell>{new Date(a.createdAt).toLocaleString()}</TableCell>
                  <TableCell>
                    {a.status === 'Open' ? (
                      <Stack direction="row" spacing={0.5}>
                        <Button size="small" color="error" onClick={() => handleReview(a.id, 'Confirmed')}>Confirm</Button>
                        <Button size="small" onClick={() => handleReview(a.id, 'Dismissed')}>Dismiss</Button>
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
