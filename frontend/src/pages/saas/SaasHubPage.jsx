import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const SECTIONS = [
  { label: 'Tenants', to: '/saas/tenants', description: 'Register companies and manage their subscription, limits, branding, domains, and database info.' },
  { label: 'Subscription Plans', to: '/saas/plans', description: 'Define plans with pricing, limits, and included features.' },
  { label: 'Billing', to: '/saas/billing', description: 'Generate and track subscription invoices across all tenants.' },
  { label: 'Usage Monitoring', to: '/saas/usage', description: 'Platform-wide KPIs: tenant growth, active subscriptions, MRR.' },
];

export default function SaasHubPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>SaaS Platform</Typography>
        <Typography color="text.secondary">
          Manages car rental companies as customers of this software. This is an additive layer — it does not gate or filter the rest of the app by tenant.
        </Typography>
        <Grid container spacing={2}>
          {SECTIONS.map((section) => (
            <Grid item xs={12} sm={6} md={4} key={section.to}>
              <Card variant="outlined">
                <CardActionArea onClick={() => navigate(section.to)} sx={{ height: '100%' }}>
                  <CardContent>
                    <Typography variant="h6">{section.label}</Typography>
                    <Typography color="text.secondary" variant="body2">{section.description}</Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </Box>
  );
}
