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
  cancelCampaign,
  createCampaign,
  deleteCampaign,
  getCampaignLogs,
  listCampaigns,
  listTemplates,
  scheduleCampaign,
  sendCampaignNow,
} from '../../services/api';

const AUDIENCES = [
  { value: 'AllCustomers', label: 'All Customers' },
  { value: 'CorporateOnly', label: 'Corporate Only' },
  { value: 'IndividualOnly', label: 'Individual Only' },
];

const statusColor = (status) => {
  switch (status) {
    case 'Draft': return 'default';
    case 'Scheduled': return 'info';
    case 'Sending': return 'warning';
    case 'Completed': return 'success';
    case 'Cancelled': return 'error';
    default: return 'default';
  }
};

export default function CampaignsPage() {
  const [campaigns, setCampaigns] = useState([]);
  const [templates, setTemplates] = useState([]);
  const [error, setError] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [form, setForm] = useState({ name: '', templateId: '', audienceFilter: 'AllCustomers' });
  const [scheduleTargetId, setScheduleTargetId] = useState(null);
  const [scheduledAt, setScheduledAt] = useState('');
  const [logsTargetId, setLogsTargetId] = useState(null);
  const [logs, setLogs] = useState([]);

  const load = () => {
    listCampaigns().then(setCampaigns).catch((err) => setError(err.message));
  };

  useEffect(load, []);
  useEffect(() => { listTemplates({}).then(setTemplates).catch(() => {}); }, []);

  const handleCreate = async () => {
    try {
      await createCampaign(form);
      setCreateOpen(false);
      setForm({ name: '', templateId: '', audienceFilter: 'AllCustomers' });
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleSendNow = async (id) => {
    try {
      await sendCampaignNow(id);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleCancel = async (id) => {
    try {
      await cancelCampaign(id);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteCampaign(id);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const openSchedule = (id) => {
    setScheduleTargetId(id);
    setScheduledAt('');
  };

  const handleSchedule = async () => {
    try {
      await scheduleCampaign(scheduleTargetId, new Date(scheduledAt).toISOString());
      setScheduleTargetId(null);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const openLogs = (id) => {
    setLogsTargetId(id);
    getCampaignLogs(id).then(setLogs).catch((err) => setError(err.message));
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Campaigns</Typography>
          <Button variant="contained" onClick={() => setCreateOpen(true)}>New Campaign</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Channel</TableCell>
                <TableCell>Audience</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Target / Sent / Failed</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {campaigns.map((c) => (
                <TableRow key={c.id} hover>
                  <TableCell>{c.name}<br /><Typography variant="caption" color="text.secondary">{c.templateName}</Typography></TableCell>
                  <TableCell><Chip size="small" label={c.channel} /></TableCell>
                  <TableCell>{c.audienceFilter}</TableCell>
                  <TableCell><Chip size="small" label={c.status} color={statusColor(c.status)} /></TableCell>
                  <TableCell>{c.targetCount} / {c.sentCount} / {c.failedCount}</TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={0.5} flexWrap="wrap">
                      {(c.status === 'Draft' || c.status === 'Scheduled') && (
                        <Button size="small" onClick={() => handleSendNow(c.id)}>Send Now</Button>
                      )}
                      {c.status === 'Draft' && (
                        <Button size="small" onClick={() => openSchedule(c.id)}>Schedule</Button>
                      )}
                      {(c.status === 'Draft' || c.status === 'Scheduled') && (
                        <Button size="small" color="error" onClick={() => handleCancel(c.id)}>Cancel</Button>
                      )}
                      {c.status === 'Draft' && (
                        <Button size="small" color="error" onClick={() => handleDelete(c.id)}>Delete</Button>
                      )}
                      <Button size="small" onClick={() => openLogs(c.id)}>Logs</Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>New Campaign</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <TextField select label="Template" value={form.templateId} onChange={(e) => setForm({ ...form, templateId: e.target.value })}>
              {templates.map((t) => <MenuItem key={t.id} value={t.id}>{t.name} ({t.channel})</MenuItem>)}
            </TextField>
            <TextField select label="Audience" value={form.audienceFilter} onChange={(e) => setForm({ ...form, audienceFilter: e.target.value })}>
              {AUDIENCES.map((a) => <MenuItem key={a.value} value={a.value}>{a.label}</MenuItem>)}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreate} disabled={!form.name || !form.templateId}>Create</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(scheduleTargetId)} onClose={() => setScheduleTargetId(null)}>
        <DialogTitle>Schedule Campaign</DialogTitle>
        <DialogContent>
          <TextField
            type="datetime-local"
            label="Send at"
            value={scheduledAt}
            onChange={(e) => setScheduledAt(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setScheduleTargetId(null)}>Cancel</Button>
          <Button variant="contained" onClick={handleSchedule} disabled={!scheduledAt}>Schedule</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(logsTargetId)} onClose={() => setLogsTargetId(null)} maxWidth="md" fullWidth>
        <DialogTitle>Campaign Logs</DialogTitle>
        <DialogContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Recipient</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Sent At</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {logs.map((log) => (
                <TableRow key={log.id}>
                  <TableCell>{log.recipientAddress}</TableCell>
                  <TableCell><Chip size="small" label={log.status} color={log.status === 'Failed' ? 'error' : 'success'} /></TableCell>
                  <TableCell>{new Date(log.sentAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setLogsTargetId(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
