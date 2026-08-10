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
  Tabs,
  Tab,
  TextField,
  Typography,
} from '@mui/material';
import { listMessageLogs, listTemplates, sendAdHocMessage } from '../../services/api';

const CHANNELS = ['Email', 'Sms', 'WhatsApp', 'Push'];

export default function MessageLogsPage() {
  const [tab, setTab] = useState(0);
  const channel = CHANNELS[tab];
  const [logs, setLogs] = useState([]);
  const [error, setError] = useState('');
  const [sendOpen, setSendOpen] = useState(false);
  const [templates, setTemplates] = useState([]);
  const [form, setForm] = useState({ recipientAddress: '', templateId: '', subject: '', body: '' });

  const load = () => {
    listMessageLogs({ channel }).then(setLogs).catch((err) => setError(err.message));
  };

  useEffect(load, [channel]);

  const openSend = () => {
    setForm({ recipientAddress: '', templateId: '', subject: '', body: '' });
    listTemplates({ channel }).then(setTemplates).catch(() => {});
    setSendOpen(true);
  };

  const handleSend = async () => {
    try {
      await sendAdHocMessage({
        channel,
        recipientAddress: form.recipientAddress,
        templateId: form.templateId || null,
        subject: form.subject || null,
        body: form.templateId ? null : form.body,
        placeholderValues: {},
      });
      setSendOpen(false);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Message Logs</Typography>
          <Button variant="contained" onClick={openSend}>Send Message</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Tabs value={tab} onChange={(_, value) => setTab(value)}>
          {CHANNELS.map((c) => <Tab key={c} label={c} />)}
        </Tabs>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Recipient</TableCell>
                <TableCell>Subject / Body</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Sent At</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {logs.map((log) => (
                <TableRow key={log.id} hover>
                  <TableCell>{log.recipientAddress}</TableCell>
                  <TableCell sx={{ maxWidth: 360 }}>{log.subject ?? log.body}</TableCell>
                  <TableCell>
                    <Chip size="small" label={log.status} color={log.status === 'Failed' ? 'error' : 'success'} />
                  </TableCell>
                  <TableCell>{new Date(log.sentAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={sendOpen} onClose={() => setSendOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Send {channel} Message</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label={channel === 'Email' ? 'Recipient email' : 'Recipient phone / address'}
              value={form.recipientAddress}
              onChange={(e) => setForm({ ...form, recipientAddress: e.target.value })}
            />
            <TextField select label="Template (optional)" value={form.templateId} onChange={(e) => setForm({ ...form, templateId: e.target.value })}>
              <MenuItem value="">None — write a custom message</MenuItem>
              {templates.map((t) => <MenuItem key={t.id} value={t.id}>{t.name}</MenuItem>)}
            </TextField>
            {!form.templateId && (
              <>
                {channel === 'Email' ? (
                  <TextField label="Subject" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
                ) : null}
                <TextField
                  label="Body"
                  multiline
                  minRows={3}
                  value={form.body}
                  onChange={(e) => setForm({ ...form, body: e.target.value })}
                />
              </>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSendOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSend} disabled={!form.recipientAddress || (!form.templateId && !form.body)}>Send</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
