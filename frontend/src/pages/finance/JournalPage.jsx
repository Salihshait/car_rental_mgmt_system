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
import { createJournalEntry, deleteJournalEntry, listJournalEntries, updateJournalEntry } from '../../services/api';

const EMPTY_FORM = { entryDate: new Date().toISOString().slice(0, 10), entryType: 'Income', category: 'General', description: '', amount: '' };
const currency = (value) => value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });

export default function JournalPage() {
  const [entries, setEntries] = useState([]);
  const [entryTypeFilter, setEntryTypeFilter] = useState('');
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);

  const load = () => {
    listJournalEntries({ entryType: entryTypeFilter }).then(setEntries).catch((err) => setError(err.message));
  };

  useEffect(load, [entryTypeFilter]);

  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setDialogOpen(true);
  };

  const openEdit = (entry) => {
    setEditingId(entry.id);
    setForm({
      entryDate: entry.entryDate.slice(0, 10),
      entryType: entry.entryType,
      category: entry.category,
      description: entry.description,
      amount: entry.amount,
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    try {
      const payload = { ...form, amount: Number(form.amount) || 0, bankAccountId: null };
      if (editingId) {
        await updateJournalEntry(editingId, payload);
      } else {
        await createJournalEntry(payload);
      }
      setDialogOpen(false);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleDelete = async (id) => {
    await deleteJournalEntry(id);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Journal</Typography>
          <Button variant="contained" onClick={openCreate}>New Entry</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TextField select size="small" label="Type" value={entryTypeFilter} onChange={(e) => setEntryTypeFilter(e.target.value)} sx={{ minWidth: 160 }}>
          <MenuItem value="">All</MenuItem>
          <MenuItem value="Income">Income</MenuItem>
          <MenuItem value="Expense">Expense</MenuItem>
        </TextField>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Description</TableCell>
                <TableCell align="right">Amount</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {entries.map((entry) => (
                <TableRow key={entry.id} hover>
                  <TableCell>{new Date(entry.entryDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <Chip size="small" label={entry.entryType} color={entry.entryType === 'Income' ? 'success' : 'error'} />
                  </TableCell>
                  <TableCell>{entry.category}</TableCell>
                  <TableCell>{entry.description}</TableCell>
                  <TableCell align="right">{currency(entry.amount)}</TableCell>
                  <TableCell>
                    <Button size="small" onClick={() => openEdit(entry)}>Edit</Button>
                    <Button size="small" color="error" onClick={() => handleDelete(entry.id)}>Delete</Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Journal Entry' : 'New Journal Entry'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              type="date"
              label="Date"
              value={form.entryDate}
              onChange={(e) => setForm({ ...form, entryDate: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField select label="Type" value={form.entryType} onChange={(e) => setForm({ ...form, entryType: e.target.value })}>
              <MenuItem value="Income">Income</MenuItem>
              <MenuItem value="Expense">Expense</MenuItem>
            </TextField>
            <TextField label="Category" value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })} />
            <TextField label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <TextField label="Amount" type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={!form.description || !form.amount}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
