import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
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
import { getComplaint, listComplaints, resolveComplaint } from '../../services/api';

const STATUSES = ['Open', 'UnderReview', 'Resolved', 'Rejected'];
const SEVERITIES = ['Low', 'Medium', 'High', 'Critical'];

const severityColor = (severity) => {
  switch (severity) {
    case 'Critical': return 'error';
    case 'High': return 'warning';
    case 'Low': return 'default';
    default: return 'info';
  }
};

const statusColor = (status) => {
  switch (status) {
    case 'Open': return 'warning';
    case 'UnderReview': return 'info';
    case 'Resolved': return 'success';
    case 'Rejected': return 'default';
    default: return 'default';
  }
};

export default function ComplaintsPage() {
  const [complaints, setComplaints] = useState([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [error, setError] = useState('');
  const [selectedId, setSelectedId] = useState(null);
  const [complaint, setComplaint] = useState(null);
  const [resolveStatus, setResolveStatus] = useState('Resolved');
  const [resolution, setResolution] = useState('');

  const load = () => {
    listComplaints({ status: statusFilter, severity: severityFilter }).then(setComplaints).catch((err) => setError(err.message));
  };

  useEffect(load, [statusFilter, severityFilter]);

  const openComplaint = (id) => {
    setSelectedId(id);
    getComplaint(id).then((c) => {
      setComplaint(c);
      setResolveStatus(c.status === 'Open' || c.status === 'UnderReview' ? 'Resolved' : c.status);
      setResolution(c.resolution ?? '');
    }).catch((err) => setError(err.message));
  };

  const closeDialog = () => {
    setSelectedId(null);
    setComplaint(null);
  };

  const handleResolve = async () => {
    await resolveComplaint(selectedId, { status: resolveStatus, resolution });
    load();
    closeDialog();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Complaints</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction="row" spacing={2}>
          <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            {STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
          </TextField>
          <TextField select size="small" label="Severity" value={severityFilter} onChange={(e) => setSeverityFilter(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            {SEVERITIES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
          </TextField>
        </Stack>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Subject</TableCell>
                <TableCell>Customer</TableCell>
                <TableCell>Severity</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {complaints.map((c) => (
                <TableRow key={c.id} hover onClick={() => openComplaint(c.id)} sx={{ cursor: 'pointer' }}>
                  <TableCell>{c.subject}</TableCell>
                  <TableCell>{c.customerName ?? '-'}</TableCell>
                  <TableCell><Chip size="small" label={c.severity} color={severityColor(c.severity)} /></TableCell>
                  <TableCell><Chip size="small" label={c.status} color={statusColor(c.status)} /></TableCell>
                  <TableCell>{new Date(c.createdAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={Boolean(selectedId)} onClose={closeDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{complaint?.subject}</DialogTitle>
        <DialogContent>
          {complaint ? (
            <Stack spacing={2}>
              <Stack direction="row" spacing={1}>
                <Chip size="small" label={complaint.severity} color={severityColor(complaint.severity)} />
                <Chip size="small" label={complaint.status} color={statusColor(complaint.status)} />
              </Stack>
              <Typography variant="body2">{complaint.description}</Typography>
              <TextField select size="small" label="Status" value={resolveStatus} onChange={(e) => setResolveStatus(e.target.value)}>
                {STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
              </TextField>
              <TextField
                label="Resolution notes"
                multiline
                minRows={3}
                value={resolution}
                onChange={(e) => setResolution(e.target.value)}
              />
            </Stack>
          ) : null}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDialog}>Cancel</Button>
          <Button variant="contained" onClick={handleResolve}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
