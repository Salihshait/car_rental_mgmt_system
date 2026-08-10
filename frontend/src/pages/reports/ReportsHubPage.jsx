import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const REPORTS = [
  { label: 'Revenue', to: '/reports/revenue', description: 'Revenue trends, branch and vehicle category breakdowns.' },
  { label: 'Bookings', to: '/reports/bookings', description: 'Booking volume, status mix, and cancellation rate.' },
  { label: 'Fleet', to: '/reports/fleet', description: 'Utilization, vehicle status, and revenue by category.' },
  { label: 'Maintenance', to: '/reports/maintenance', description: 'Maintenance cost, vendor performance, open work orders.' },
  { label: 'Customer', to: '/reports/customers', description: 'Customer growth, segmentation, and top spenders.' },
  { label: 'Driver', to: '/reports/drivers', description: 'Driver ratings, attendance, and salary payouts.' },
  { label: 'Finance', to: '/reports/finance', description: 'Revenue vs expenses, payment methods, outstanding invoices.' },
];

export default function ReportsHubPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Reports</Typography>
        <Grid container spacing={2}>
          {REPORTS.map((report) => (
            <Grid item xs={12} sm={6} md={4} key={report.to}>
              <Card variant="outlined">
                <CardActionArea onClick={() => navigate(report.to)} sx={{ height: '100%' }}>
                  <CardContent>
                    <Typography variant="h6">{report.label}</Typography>
                    <Typography color="text.secondary" variant="body2">{report.description}</Typography>
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
