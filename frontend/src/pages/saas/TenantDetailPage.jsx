import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
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
  TableHead,
  TableRow,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import {
  cancelTenantSubscription,
  createTenantDomain,
  createTenantSubscription,
  getTenant,
  getTenantBranding,
  getTenantDatabaseInfo,
  getTenantEffectiveLimits,
  listSubscriptionPlans,
  listTenantDomains,
  listTenantFeatureOverrides,
  listTenantSubscriptions,
  updateTenant,
  updateTenantBranding,
  upsertTenantFeatureOverride,
  verifyTenantDomain,
} from '../../services/api';

const TABS = ['Overview', 'Subscription', 'Plan Limits', 'Feature Toggles', 'White Label', 'Custom Domain', 'Database'];

function OverviewTab({ tenant, onSave }) {
  const [form, setForm] = useState({ companyName: tenant.companyName, contactEmail: tenant.contactEmail, contactPhone: tenant.contactPhone ?? '', status: tenant.status });

  return (
    <Stack spacing={2} sx={{ maxWidth: 480 }}>
      <TextField label="Company Name" value={form.companyName} onChange={(e) => setForm({ ...form, companyName: e.target.value })} />
      <TextField label="Contact Email" value={form.contactEmail} onChange={(e) => setForm({ ...form, contactEmail: e.target.value })} />
      <TextField label="Contact Phone" value={form.contactPhone} onChange={(e) => setForm({ ...form, contactPhone: e.target.value })} />
      <TextField select label="Status" value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
        {['Trial', 'Active', 'Suspended', 'Cancelled'].map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
      </TextField>
      {tenant.trialEndsAt ? <Typography variant="body2" color="text.secondary">Trial ends {new Date(tenant.trialEndsAt).toLocaleDateString()}</Typography> : null}
      <Button variant="contained" sx={{ alignSelf: 'flex-start' }} onClick={() => onSave(form)}>Save</Button>
    </Stack>
  );
}

function SubscriptionTab({ tenantId }) {
  const [subscriptions, setSubscriptions] = useState([]);
  const [plans, setPlans] = useState([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState({ planId: '', billingCycle: 'Monthly' });

  const load = () => listTenantSubscriptions(tenantId).then(setSubscriptions);
  useEffect(load, [tenantId]);
  useEffect(() => { listSubscriptionPlans({ activeOnly: true }).then(setPlans); }, []);

  const handleCreate = async () => {
    await createTenantSubscription(tenantId, form);
    setDialogOpen(false);
    load();
  };

  const handleCancel = async (subscriptionId) => {
    await cancelTenantSubscription(tenantId, subscriptionId);
    load();
  };

  return (
    <Stack spacing={2}>
      <Button variant="contained" sx={{ alignSelf: 'flex-start' }} onClick={() => setDialogOpen(true)}>Assign Subscription</Button>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Plan</TableCell>
            <TableCell>Billing Cycle</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Period</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {subscriptions.map((s) => (
            <TableRow key={s.id}>
              <TableCell>{s.planName}</TableCell>
              <TableCell>{s.billingCycle}</TableCell>
              <TableCell><Chip size="small" label={s.status} color={s.status === 'Active' ? 'success' : 'default'} /></TableCell>
              <TableCell>{new Date(s.currentPeriodStart).toLocaleDateString()} - {new Date(s.currentPeriodEnd).toLocaleDateString()}</TableCell>
              <TableCell>
                {s.status === 'Active' ? <Button size="small" color="error" onClick={() => handleCancel(s.id)}>Cancel</Button> : null}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Assign Subscription</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField select label="Plan" value={form.planId} onChange={(e) => setForm({ ...form, planId: e.target.value })}>
              {plans.map((p) => <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>)}
            </TextField>
            <TextField select label="Billing Cycle" value={form.billingCycle} onChange={(e) => setForm({ ...form, billingCycle: e.target.value })}>
              <MenuItem value="Monthly">Monthly</MenuItem>
              <MenuItem value="Annual">Annual</MenuItem>
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreate} disabled={!form.planId}>Assign</Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}

function PlanLimitsTab({ tenantId }) {
  const [limits, setLimits] = useState([]);
  useEffect(() => { getTenantEffectiveLimits(tenantId).then(setLimits); }, [tenantId]);

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Limit</TableCell>
          <TableCell>Value</TableCell>
          <TableCell>From Plan</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {limits.map((l) => (
          <TableRow key={l.limitKey}>
            <TableCell>{l.limitKey}</TableCell>
            <TableCell>{l.limitValue < 0 ? 'Unlimited' : l.limitValue}</TableCell>
            <TableCell>{l.planName ?? '-'}</TableCell>
          </TableRow>
        ))}
        {limits.length === 0 ? (
          <TableRow><TableCell colSpan={3}><Typography color="text.secondary" variant="body2">No active subscription — no effective limits.</Typography></TableCell></TableRow>
        ) : null}
      </TableBody>
    </Table>
  );
}

function FeatureTogglesTab({ tenantId }) {
  const [overrides, setOverrides] = useState([]);
  const [featureKey, setFeatureKey] = useState('');
  const [isEnabled, setIsEnabled] = useState(true);

  const load = () => listTenantFeatureOverrides(tenantId).then(setOverrides);
  useEffect(load, [tenantId]);

  const handleAdd = async () => {
    await upsertTenantFeatureOverride(tenantId, { featureKey, isEnabled });
    setFeatureKey('');
    load();
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={1} alignItems="center">
        <TextField size="small" label="Feature key" value={featureKey} onChange={(e) => setFeatureKey(e.target.value)} />
        <TextField select size="small" label="Enabled" value={isEnabled} onChange={(e) => setIsEnabled(e.target.value === 'true')} sx={{ minWidth: 120 }}>
          <MenuItem value="true">Enabled</MenuItem>
          <MenuItem value="false">Disabled</MenuItem>
        </TextField>
        <Button variant="contained" onClick={handleAdd} disabled={!featureKey}>Set Override</Button>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Feature</TableCell>
            <TableCell>Override</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {overrides.map((o) => (
            <TableRow key={o.id}>
              <TableCell>{o.featureKey}</TableCell>
              <TableCell><Chip size="small" label={o.isEnabled ? 'Enabled' : 'Disabled'} color={o.isEnabled ? 'success' : 'default'} /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Stack>
  );
}

function WhiteLabelTab({ tenantId }) {
  const [form, setForm] = useState({ logoUrl: '', primaryColor: '', secondaryColor: '', companyDisplayName: '', faviconUrl: '' });

  useEffect(() => {
    getTenantBranding(tenantId).then((b) => setForm({
      logoUrl: b.logoUrl ?? '', primaryColor: b.primaryColor ?? '', secondaryColor: b.secondaryColor ?? '',
      companyDisplayName: b.companyDisplayName ?? '', faviconUrl: b.faviconUrl ?? '',
    }));
  }, [tenantId]);

  const handleSave = async () => {
    await updateTenantBranding(tenantId, form);
  };

  return (
    <Stack spacing={2} sx={{ maxWidth: 480 }}>
      <TextField label="Company Display Name" value={form.companyDisplayName} onChange={(e) => setForm({ ...form, companyDisplayName: e.target.value })} />
      <TextField label="Logo URL" value={form.logoUrl} onChange={(e) => setForm({ ...form, logoUrl: e.target.value })} />
      <TextField label="Favicon URL" value={form.faviconUrl} onChange={(e) => setForm({ ...form, faviconUrl: e.target.value })} />
      <Stack direction="row" spacing={2}>
        <TextField label="Primary Color" value={form.primaryColor} onChange={(e) => setForm({ ...form, primaryColor: e.target.value })} fullWidth />
        <TextField label="Secondary Color" value={form.secondaryColor} onChange={(e) => setForm({ ...form, secondaryColor: e.target.value })} fullWidth />
      </Stack>
      <Button variant="contained" sx={{ alignSelf: 'flex-start' }} onClick={handleSave}>Save Branding</Button>
    </Stack>
  );
}

function CustomDomainTab({ tenantId }) {
  const [domains, setDomains] = useState([]);
  const [newDomain, setNewDomain] = useState('');

  const load = () => listTenantDomains(tenantId).then(setDomains);
  useEffect(load, [tenantId]);

  const handleAdd = async () => {
    await createTenantDomain(tenantId, { domain: newDomain });
    setNewDomain('');
    load();
  };

  const handleVerify = async (domainId) => {
    await verifyTenantDomain(tenantId, domainId);
    load();
  };

  return (
    <Stack spacing={2}>
      <Alert severity="info">Domain status is manual — there is no live DNS/SSL verification from this environment.</Alert>
      <Stack direction="row" spacing={1}>
        <TextField size="small" label="Domain" placeholder="rentals.example.com" value={newDomain} onChange={(e) => setNewDomain(e.target.value)} />
        <Button variant="contained" onClick={handleAdd} disabled={!newDomain}>Add Domain</Button>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Domain</TableCell>
            <TableCell>Status</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {domains.map((d) => (
            <TableRow key={d.id}>
              <TableCell>{d.domain}</TableCell>
              <TableCell><Chip size="small" label={d.status} color={d.status === 'Verified' ? 'success' : 'warning'} /></TableCell>
              <TableCell>
                {d.status !== 'Verified' ? <Button size="small" onClick={() => handleVerify(d.id)}>Mark Verified</Button> : null}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Stack>
  );
}

function DatabaseTab({ tenantId }) {
  const [info, setInfo] = useState(null);
  useEffect(() => { getTenantDatabaseInfo(tenantId).then(setInfo); }, [tenantId]);

  if (!info) return null;

  return (
    <Stack spacing={2}>
      <Alert severity="info">{info.isolationModel}</Alert>
      <Table size="small" sx={{ maxWidth: 480 }}>
        <TableBody>
          <TableRow><TableCell>Subscriptions</TableCell><TableCell align="right">{info.subscriptionCount}</TableCell></TableRow>
          <TableRow><TableCell>Invoices</TableCell><TableCell align="right">{info.invoiceCount}</TableCell></TableRow>
          <TableRow><TableCell>Usage metric records</TableCell><TableCell align="right">{info.usageMetricRecordCount}</TableCell></TableRow>
        </TableBody>
      </Table>
    </Stack>
  );
}

export default function TenantDetailPage() {
  const { id } = useParams();
  const [tenant, setTenant] = useState(null);
  const [tab, setTab] = useState(0);
  const [error, setError] = useState('');

  const load = () => getTenant(id).then(setTenant).catch((err) => setError(err.message));
  useEffect(load, [id]);

  const handleSaveOverview = async (form) => {
    try {
      await updateTenant(id, form);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  if (error) return <Box sx={{ p: 4 }}><Alert severity="error">{error}</Alert></Box>;
  if (!tenant) return null;

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>{tenant.companyName}</Typography>
        <Tabs value={tab} onChange={(_, value) => setTab(value)} variant="scrollable">
          {TABS.map((label) => <Tab key={label} label={label} />)}
        </Tabs>

        <Box>
          {tab === 0 ? <OverviewTab tenant={tenant} onSave={handleSaveOverview} /> : null}
          {tab === 1 ? <SubscriptionTab tenantId={id} /> : null}
          {tab === 2 ? <PlanLimitsTab tenantId={id} /> : null}
          {tab === 3 ? <FeatureTogglesTab tenantId={id} /> : null}
          {tab === 4 ? <WhiteLabelTab tenantId={id} /> : null}
          {tab === 5 ? <CustomDomainTab tenantId={id} /> : null}
          {tab === 6 ? <DatabaseTab tenantId={id} /> : null}
        </Box>
      </Stack>
    </Box>
  );
}
