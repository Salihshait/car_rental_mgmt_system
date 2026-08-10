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
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { createTemplate, deleteTemplate, listTemplates, previewTemplate, updateTemplate } from '../../services/api';

const CHANNELS = ['Email', 'Sms', 'WhatsApp', 'Push'];
const EMPTY_FORM = { name: '', channel: 'Email', subject: '', body: '', isActive: true };

export default function MessageTemplatesPage() {
  const [templates, setTemplates] = useState([]);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [preview, setPreview] = useState(null);

  const load = () => {
    listTemplates().then(setTemplates).catch((err) => setError(err.message));
  };

  useEffect(load, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setPreview(null);
    setDialogOpen(true);
  };

  const openEdit = (template) => {
    setEditingId(template.id);
    setForm({ name: template.name, channel: template.channel, subject: template.subject ?? '', body: template.body, isActive: template.isActive });
    setPreview(null);
    setDialogOpen(true);
  };

  const handleSave = async () => {
    const payload = { ...form, subject: form.channel === 'Email' ? form.subject : null };
    if (editingId) {
      await updateTemplate(editingId, payload);
    } else {
      await createTemplate(payload);
    }
    setDialogOpen(false);
    load();
  };

  const handleDelete = async (id) => {
    await deleteTemplate(id);
    load();
  };

  const handlePreview = async () => {
    if (!editingId) return;
    const result = await previewTemplate(editingId, {});
    setPreview(result);
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Message Templates</Typography>
          <Button variant="contained" onClick={openCreate}>New Template</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Channel</TableCell>
                <TableCell>Active</TableCell>
                <TableCell>Updated</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {templates.map((t) => (
                <TableRow key={t.id} hover>
                  <TableCell>{t.name}</TableCell>
                  <TableCell><Chip size="small" label={t.channel} /></TableCell>
                  <TableCell>
                    <Chip size="small" label={t.isActive ? 'Active' : 'Inactive'} color={t.isActive ? 'success' : 'default'} />
                  </TableCell>
                  <TableCell>{t.updatedAt ? new Date(t.updatedAt).toLocaleString() : '-'}</TableCell>
                  <TableCell>
                    <Button size="small" onClick={() => openEdit(t)}>Edit</Button>
                    <Button size="small" color="error" onClick={() => handleDelete(t.id)}>Delete</Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Template' : 'New Template'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <TextField select label="Channel" value={form.channel} onChange={(e) => setForm({ ...form, channel: e.target.value })}>
              {CHANNELS.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
            </TextField>
            {form.channel === 'Email' ? (
              <TextField label="Subject" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
            ) : null}
            <TextField
              label="Body"
              helperText="Use {{CustomerName}}, {{BookingId}}, {{CompanyName}} as placeholders."
              multiline
              minRows={4}
              value={form.body}
              onChange={(e) => setForm({ ...form, body: e.target.value })}
            />
            <FormControlLabel
              control={<Switch checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />}
              label="Active"
            />
            {editingId ? (
              <Stack spacing={1}>
                <Button size="small" onClick={handlePreview}>Preview with sample data</Button>
                {preview ? (
                  <Box sx={{ p: 1.5, bgcolor: 'action.hover', borderRadius: 1 }}>
                    {preview.subject ? <Typography variant="subtitle2">{preview.subject}</Typography> : null}
                    <Typography variant="body2">{preview.body}</Typography>
                  </Box>
                ) : null}
              </Stack>
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
