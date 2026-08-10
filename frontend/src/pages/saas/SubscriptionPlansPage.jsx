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
  IconButton,
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
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { createSubscriptionPlan, listSubscriptionPlans, updateSubscriptionPlan } from '../../services/api';

const EMPTY_FORM = { name: '', description: '', monthlyPrice: 0, annualPrice: 0, currency: 'USD', isActive: true, limits: [], features: [] };
const currency = (value, code) => value.toLocaleString(undefined, { style: 'currency', currency: code || 'USD' });

export default function SubscriptionPlansPage() {
  const [plans, setPlans] = useState([]);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);

  const load = () => {
    listSubscriptionPlans().then(setPlans).catch((err) => setError(err.message));
  };

  useEffect(load, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setDialogOpen(true);
  };

  const openEdit = (plan) => {
    setEditingId(plan.id);
    setForm({
      name: plan.name,
      description: plan.description ?? '',
      monthlyPrice: plan.monthlyPrice,
      annualPrice: plan.annualPrice,
      currency: plan.currency,
      isActive: plan.isActive,
      limits: plan.limits.map((l) => ({ limitKey: l.limitKey, limitValue: l.limitValue })),
      features: plan.features.map((f) => ({ featureKey: f.featureKey, isEnabled: f.isEnabled })),
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    try {
      const payload = {
        ...form,
        monthlyPrice: Number(form.monthlyPrice) || 0,
        annualPrice: Number(form.annualPrice) || 0,
      };
      if (editingId) {
        await updateSubscriptionPlan(editingId, payload);
      } else {
        await createSubscriptionPlan(payload);
      }
      setDialogOpen(false);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const addLimit = () => setForm({ ...form, limits: [...form.limits, { limitKey: '', limitValue: 0 }] });
  const updateLimit = (index, field, value) => {
    const limits = [...form.limits];
    limits[index] = { ...limits[index], [field]: value };
    setForm({ ...form, limits });
  };
  const removeLimit = (index) => setForm({ ...form, limits: form.limits.filter((_, i) => i !== index) });

  const addFeature = () => setForm({ ...form, features: [...form.features, { featureKey: '', isEnabled: true }] });
  const updateFeature = (index, field, value) => {
    const features = [...form.features];
    features[index] = { ...features[index], [field]: value };
    setForm({ ...form, features });
  };
  const removeFeature = (index) => setForm({ ...form, features: form.features.filter((_, i) => i !== index) });

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Subscription Plans</Typography>
          <Button variant="contained" onClick={openCreate}>New Plan</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell align="right">Monthly</TableCell>
                <TableCell align="right">Annual</TableCell>
                <TableCell>Limits</TableCell>
                <TableCell>Features</TableCell>
                <TableCell>Status</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {plans.map((plan) => (
                <TableRow key={plan.id} hover>
                  <TableCell>{plan.name}</TableCell>
                  <TableCell align="right">{currency(plan.monthlyPrice, plan.currency)}</TableCell>
                  <TableCell align="right">{currency(plan.annualPrice, plan.currency)}</TableCell>
                  <TableCell>{plan.limits.length}</TableCell>
                  <TableCell>{plan.features.filter((f) => f.isEnabled).length} / {plan.features.length}</TableCell>
                  <TableCell>
                    <Chip size="small" label={plan.isActive ? 'Active' : 'Inactive'} color={plan.isActive ? 'success' : 'default'} />
                  </TableCell>
                  <TableCell>
                    <Button size="small" onClick={() => openEdit(plan)}>Edit</Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Plan' : 'New Plan'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <TextField label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <Stack direction="row" spacing={2}>
              <TextField label="Monthly Price" type="number" value={form.monthlyPrice} onChange={(e) => setForm({ ...form, monthlyPrice: e.target.value })} fullWidth />
              <TextField label="Annual Price" type="number" value={form.annualPrice} onChange={(e) => setForm({ ...form, annualPrice: e.target.value })} fullWidth />
            </Stack>
            <FormControlLabel
              control={<Switch checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />}
              label="Active"
            />

            <Stack spacing={1}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle2">Limits</Typography>
                <IconButton size="small" onClick={addLimit}><AddIcon fontSize="small" /></IconButton>
              </Stack>
              {form.limits.map((limit, index) => (
                <Stack direction="row" spacing={1} key={index} alignItems="center">
                  <TextField size="small" label="Key" value={limit.limitKey} onChange={(e) => updateLimit(index, 'limitKey', e.target.value)} />
                  <TextField size="small" label="Value (-1 = unlimited)" type="number" value={limit.limitValue} onChange={(e) => updateLimit(index, 'limitValue', Number(e.target.value))} />
                  <IconButton size="small" onClick={() => removeLimit(index)}><DeleteIcon fontSize="small" /></IconButton>
                </Stack>
              ))}
            </Stack>

            <Stack spacing={1}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle2">Features</Typography>
                <IconButton size="small" onClick={addFeature}><AddIcon fontSize="small" /></IconButton>
              </Stack>
              {form.features.map((feature, index) => (
                <Stack direction="row" spacing={1} key={index} alignItems="center">
                  <TextField size="small" label="Key" value={feature.featureKey} onChange={(e) => updateFeature(index, 'featureKey', e.target.value)} />
                  <FormControlLabel
                    control={<Switch checked={feature.isEnabled} onChange={(e) => updateFeature(index, 'isEnabled', e.target.checked)} />}
                    label="Enabled"
                  />
                  <IconButton size="small" onClick={() => removeFeature(index)}><DeleteIcon fontSize="small" /></IconButton>
                </Stack>
              ))}
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={!form.name}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
