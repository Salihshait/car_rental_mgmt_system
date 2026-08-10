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
import {
  assignTicket,
  getMyProfile,
  getTicketDetail,
  listTickets,
  replyToTicketAsStaff,
  updateTicketStatus,
} from '../../services/api';

const STATUSES = ['Open', 'InProgress', 'Resolved', 'Closed'];
const PRIORITIES = ['Low', 'Normal', 'High', 'Urgent'];

const priorityColor = (priority) => {
  switch (priority) {
    case 'Urgent': return 'error';
    case 'High': return 'warning';
    case 'Low': return 'default';
    default: return 'info';
  }
};

const statusColor = (status) => {
  switch (status) {
    case 'Open': return 'warning';
    case 'InProgress': return 'info';
    case 'Resolved': return 'success';
    case 'Closed': return 'default';
    default: return 'default';
  }
};

export default function SupportTicketsPage() {
  const [tickets, setTickets] = useState([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [error, setError] = useState('');
  const [me, setMe] = useState(null);
  const [selectedId, setSelectedId] = useState(null);
  const [detail, setDetail] = useState(null);
  const [reply, setReply] = useState('');

  const load = () => {
    listTickets({ status: statusFilter, priority: priorityFilter }).then(setTickets).catch((err) => setError(err.message));
  };

  useEffect(load, [statusFilter, priorityFilter]);
  useEffect(() => { getMyProfile().then(setMe).catch(() => {}); }, []);

  const openTicket = (id) => {
    setSelectedId(id);
    getTicketDetail(id).then(setDetail).catch((err) => setError(err.message));
  };

  const closeDialog = () => {
    setSelectedId(null);
    setDetail(null);
    setReply('');
  };

  const handleReply = async () => {
    if (!reply.trim()) return;
    await replyToTicketAsStaff(selectedId, { message: reply, isInternalNote: false });
    setReply('');
    openTicket(selectedId);
    load();
  };

  const handleStatusChange = async (status) => {
    await updateTicketStatus(selectedId, status);
    openTicket(selectedId);
    load();
  };

  const handleAssignToMe = async () => {
    if (!me) return;
    await assignTicket(selectedId, me.id);
    openTicket(selectedId);
    load();
  };

  const handleUnassign = async () => {
    await assignTicket(selectedId, null);
    openTicket(selectedId);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Support Tickets</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction="row" spacing={2}>
          <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            {STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
          </TextField>
          <TextField select size="small" label="Priority" value={priorityFilter} onChange={(e) => setPriorityFilter(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            {PRIORITIES.map((p) => <MenuItem key={p} value={p}>{p}</MenuItem>)}
          </TextField>
        </Stack>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Subject</TableCell>
                <TableCell>Customer</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Assigned To</TableCell>
                <TableCell>Created</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tickets.map((ticket) => (
                <TableRow key={ticket.id} hover onClick={() => openTicket(ticket.id)} sx={{ cursor: 'pointer' }}>
                  <TableCell>{ticket.subject}</TableCell>
                  <TableCell>{ticket.customerName ?? '-'}</TableCell>
                  <TableCell>{ticket.category}</TableCell>
                  <TableCell><Chip size="small" label={ticket.priority} color={priorityColor(ticket.priority)} /></TableCell>
                  <TableCell><Chip size="small" label={ticket.status} color={statusColor(ticket.status)} /></TableCell>
                  <TableCell>{ticket.assignedToName ?? '-'}</TableCell>
                  <TableCell>{new Date(ticket.createdAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={Boolean(selectedId)} onClose={closeDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{detail?.ticket.subject}</DialogTitle>
        <DialogContent>
          {detail ? (
            <Stack spacing={2}>
              <Stack direction="row" spacing={1}>
                <Chip size="small" label={detail.ticket.status} color={statusColor(detail.ticket.status)} />
                <Chip size="small" label={detail.ticket.priority} color={priorityColor(detail.ticket.priority)} />
                <Chip size="small" label={detail.ticket.category} variant="outlined" />
              </Stack>

              <Stack direction="row" spacing={1}>
                <TextField select size="small" label="Status" value={detail.ticket.status} onChange={(e) => handleStatusChange(e.target.value)} sx={{ minWidth: 160 }}>
                  {STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </TextField>
                {detail.ticket.assignedToUserId ? (
                  <Button size="small" onClick={handleUnassign}>Unassign</Button>
                ) : (
                  <Button size="small" onClick={handleAssignToMe}>Assign to me</Button>
                )}
              </Stack>

              <Stack spacing={1} sx={{ maxHeight: 260, overflowY: 'auto' }}>
                {detail.messages.map((message) => (
                  <Box key={message.id} sx={{ p: 1, borderRadius: 1, bgcolor: message.isInternalNote ? 'warning.light' : 'action.hover' }}>
                    <Typography variant="caption" color="text.secondary">{message.senderName} · {new Date(message.createdAt).toLocaleString()}</Typography>
                    <Typography variant="body2">{message.message}</Typography>
                  </Box>
                ))}
              </Stack>

              <TextField
                label="Reply"
                multiline
                minRows={2}
                value={reply}
                onChange={(e) => setReply(e.target.value)}
              />
            </Stack>
          ) : null}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDialog}>Close</Button>
          <Button variant="contained" onClick={handleReply}>Send Reply</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
