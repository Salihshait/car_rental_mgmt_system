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
import { useNavigate } from 'react-router-dom';
import { listTenants, registerTenant } from '../../services/api';

const EMPTY_FORM = { companyName: '', slug: '', contactEmail: '', contactPhone: '' };

const statusColor = (status) => {
  switch (status) {
    case 'Trial': return 'info';
    case 'Active': return 'success';
    case 'Suspended': return 'warning';
    case 'Cancelled': return 'default';
    default: return 'default';
  }
};

export default function TenantsPage() {
  const navigate = useNavigate();
  const [tenants, setTenants] = useState([]);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);

  const load = () => {
    listTenants().then(setTenants).catch((err) => setError(err.message));
  };

  useEffect(load, []);

  const slugify = (value) => value.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');

  const handleRegister = async () => {
    try {
      await registerTenant(form);
      setDialogOpen(false);
      setForm(EMPTY_FORM);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Tenants</Typography>
          <Button variant="contained" onClick={() => setDialogOpen(true)}>Register Company</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Company</TableCell>
                <TableCell>Slug</TableCell>
                <TableCell>Contact</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Trial Ends</TableCell>
                <TableCell>Created</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tenants.map((tenant) => (
                <TableRow key={tenant.id} hover onClick={() => navigate(`/saas/tenants/${tenant.id}`)} sx={{ cursor: 'pointer' }}>
                  <TableCell>{tenant.companyName}</TableCell>
                  <TableCell>{tenant.slug}</TableCell>
                  <TableCell>{tenant.contactEmail}</TableCell>
                  <TableCell><Chip size="small" label={tenant.status} color={statusColor(tenant.status)} /></TableCell>
                  <TableCell>{tenant.trialEndsAt ? new Date(tenant.trialEndsAt).toLocaleDateString() : '-'}</TableCell>
                  <TableCell>{new Date(tenant.createdAt).toLocaleDateString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Register Company</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Company Name"
              value={form.companyName}
              onChange={(e) => setForm({ ...form, companyName: e.target.value, slug: form.slug || slugify(e.target.value) })}
            />
            <TextField label="Slug" value={form.slug} onChange={(e) => setForm({ ...form, slug: slugify(e.target.value) })} helperText="Used as a unique, URL-safe identifier." />
            <TextField label="Contact Email" value={form.contactEmail} onChange={(e) => setForm({ ...form, contactEmail: e.target.value })} />
            <TextField label="Contact Phone" value={form.contactPhone} onChange={(e) => setForm({ ...form, contactPhone: e.target.value })} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleRegister} disabled={!form.companyName || !form.slug || !form.contactEmail}>Register</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
