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
import { createTicket, getMyTicketDetail, listMyTickets, replyToTicketAsCustomer } from '../services/api';

const CATEGORIES = ['General', 'Billing', 'Technical', 'Vehicle', 'Other'];
const PRIORITIES = ['Low', 'Normal', 'High', 'Urgent'];

const statusColor = (status) => {
  switch (status) {
    case 'Open': return 'warning';
    case 'InProgress': return 'info';
    case 'Resolved': return 'success';
    case 'Closed': return 'default';
    default: return 'default';
  }
};

export default function MyTicketsPage() {
  const [tickets, setTickets] = useState([]);
  const [error, setError] = useState('');
  const [newOpen, setNewOpen] = useState(false);
  const [form, setForm] = useState({ subject: '', category: 'General', priority: 'Normal', message: '' });
  const [selectedId, setSelectedId] = useState(null);
  const [detail, setDetail] = useState(null);
  const [reply, setReply] = useState('');

  const load = () => {
    listMyTickets().then(setTickets).catch((err) => setError(err.message));
  };

  useEffect(load, []);

  const handleCreate = async () => {
    try {
      await createTicket(form);
      setNewOpen(false);
      setForm({ subject: '', category: 'General', priority: 'Normal', message: '' });
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const openTicket = (id) => {
    setSelectedId(id);
    getMyTicketDetail(id).then(setDetail).catch((err) => setError(err.message));
  };

  const closeDialog = () => {
    setSelectedId(null);
    setDetail(null);
    setReply('');
  };

  const handleReply = async () => {
    if (!reply.trim()) return;
    await replyToTicketAsCustomer(selectedId, { message: reply });
    setReply('');
    openTicket(selectedId);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>My Support Tickets</Typography>
          <Button variant="contained" onClick={() => setNewOpen(true)}>New Ticket</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Subject</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tickets.map((ticket) => (
                <TableRow key={ticket.id} hover onClick={() => openTicket(ticket.id)} sx={{ cursor: 'pointer' }}>
                  <TableCell>{ticket.subject}</TableCell>
                  <TableCell>{ticket.category}</TableCell>
                  <TableCell>{ticket.priority}</TableCell>
                  <TableCell><Chip size="small" label={ticket.status} color={statusColor(ticket.status)} /></TableCell>
                  <TableCell>{new Date(ticket.createdAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={newOpen} onClose={() => setNewOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>New Support Ticket</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Subject" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
            <TextField select label="Category" value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
              {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
            </TextField>
            <TextField select label="Priority" value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
              {PRIORITIES.map((p) => <MenuItem key={p} value={p}>{p}</MenuItem>)}
            </TextField>
            <TextField
              label="Describe your issue"
              multiline
              minRows={3}
              value={form.message}
              onChange={(e) => setForm({ ...form, message: e.target.value })}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setNewOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreate} disabled={!form.subject || !form.message}>Submit</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(selectedId)} onClose={closeDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{detail?.ticket.subject}</DialogTitle>
        <DialogContent>
          {detail ? (
            <Stack spacing={2}>
              <Chip size="small" label={detail.ticket.status} color={statusColor(detail.ticket.status)} sx={{ width: 'fit-content' }} />
              <Stack spacing={1} sx={{ maxHeight: 260, overflowY: 'auto' }}>
                {detail.messages.filter((m) => !m.isInternalNote).map((message) => (
                  <Box key={message.id} sx={{ p: 1, borderRadius: 1, bgcolor: 'action.hover' }}>
                    <Typography variant="caption" color="text.secondary">{message.senderName} · {new Date(message.createdAt).toLocaleString()}</Typography>
                    <Typography variant="body2">{message.message}</Typography>
                  </Box>
                ))}
              </Stack>
              <TextField label="Reply" multiline minRows={2} value={reply} onChange={(e) => setReply(e.target.value)} />
            </Stack>
          ) : null}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDialog}>Close</Button>
          <Button variant="contained" onClick={handleReply}>Send</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
