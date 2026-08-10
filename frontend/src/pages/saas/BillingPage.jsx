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
import { generateSubscriptionInvoice, listSubscriptionInvoices, listTenants, listTenantSubscriptions, markSubscriptionInvoicePaid } from '../../services/api';

const currency = (value, code) => value.toLocaleString(undefined, { style: 'currency', currency: code || 'USD' });

const statusColor = (status) => {
  switch (status) {
    case 'Paid': return 'success';
    case 'Pending': return 'warning';
    case 'Failed': return 'error';
    case 'Refunded': return 'default';
    default: return 'default';
  }
};

export default function BillingPage() {
  const [invoices, setInvoices] = useState([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [error, setError] = useState('');
  const [generateOpen, setGenerateOpen] = useState(false);
  const [tenants, setTenants] = useState([]);
  const [selectedTenantId, setSelectedTenantId] = useState('');
  const [subscriptions, setSubscriptions] = useState([]);
  const [selectedSubscriptionId, setSelectedSubscriptionId] = useState('');
  const [payDialogInvoiceId, setPayDialogInvoiceId] = useState(null);
  const [gateway, setGateway] = useState('Cash');

  const load = () => {
    listSubscriptionInvoices({ status: statusFilter }).then(setInvoices).catch((err) => setError(err.message));
  };

  useEffect(load, [statusFilter]);
  useEffect(() => { listTenants().then(setTenants).catch(() => {}); }, []);

  useEffect(() => {
    if (selectedTenantId) {
      listTenantSubscriptions(selectedTenantId).then(setSubscriptions);
    } else {
      setSubscriptions([]);
    }
    setSelectedSubscriptionId('');
  }, [selectedTenantId]);

  const handleGenerate = async () => {
    try {
      await generateSubscriptionInvoice(selectedSubscriptionId);
      setGenerateOpen(false);
      setSelectedTenantId('');
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleMarkPaid = async () => {
    await markSubscriptionInvoicePaid(payDialogInvoiceId, gateway);
    setPayDialogInvoiceId(null);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Billing</Typography>
          <Button variant="contained" onClick={() => setGenerateOpen(true)}>Generate Invoice</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TextField select size="small" label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} sx={{ minWidth: 160 }}>
          <MenuItem value="">All</MenuItem>
          {['Pending', 'Paid', 'Failed', 'Refunded'].map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </TextField>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Invoice</TableCell>
                <TableCell>Tenant</TableCell>
                <TableCell>Period</TableCell>
                <TableCell align="right">Amount</TableCell>
                <TableCell>Status</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {invoices.map((invoice) => (
                <TableRow key={invoice.id} hover>
                  <TableCell>{invoice.invoiceNumber}</TableCell>
                  <TableCell>{invoice.tenantName ?? '-'}</TableCell>
                  <TableCell>{new Date(invoice.periodStart).toLocaleDateString()} - {new Date(invoice.periodEnd).toLocaleDateString()}</TableCell>
                  <TableCell align="right">{currency(invoice.amount, invoice.currency)}</TableCell>
                  <TableCell><Chip size="small" label={invoice.status} color={statusColor(invoice.status)} /></TableCell>
                  <TableCell>
                    {invoice.status === 'Pending' ? <Button size="small" onClick={() => setPayDialogInvoiceId(invoice.id)}>Mark Paid</Button> : null}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>

      <Dialog open={generateOpen} onClose={() => setGenerateOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Invoice</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField select label="Tenant" value={selectedTenantId} onChange={(e) => setSelectedTenantId(e.target.value)}>
              {tenants.map((t) => <MenuItem key={t.id} value={t.id}>{t.companyName}</MenuItem>)}
            </TextField>
            <TextField select label="Subscription" value={selectedSubscriptionId} onChange={(e) => setSelectedSubscriptionId(e.target.value)} disabled={!subscriptions.length}>
              {subscriptions.map((s) => <MenuItem key={s.id} value={s.id}>{s.planName} ({s.billingCycle})</MenuItem>)}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setGenerateOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleGenerate} disabled={!selectedSubscriptionId}>Generate</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(payDialogInvoiceId)} onClose={() => setPayDialogInvoiceId(null)}>
        <DialogTitle>Mark Invoice Paid</DialogTitle>
        <DialogContent>
          <TextField select label="Gateway" value={gateway} onChange={(e) => setGateway(e.target.value)} sx={{ mt: 1, minWidth: 200 }}>
            {['Cash', 'Razorpay', 'Stripe', 'Upi'].map((g) => <MenuItem key={g} value={g}>{g}</MenuItem>)}
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPayDialogInvoiceId(null)}>Cancel</Button>
          <Button variant="contained" onClick={handleMarkPaid}>Confirm</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
