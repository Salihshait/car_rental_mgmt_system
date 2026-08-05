import { Box, Button, Card, CardContent, Grid, Typography, Stack } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const stats = [
  { label: 'Revenue', value: '$1.2M' },
  { label: 'Bookings', value: '1,248' },
  { label: 'Active Rentals', value: '128' },
  { label: 'Available Cars', value: '94' },
];

export default function DashboardPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Dashboard</Typography>
          <Stack direction="row" spacing={2}>
            <Button variant="outlined" onClick={() => navigate('/booking')}>Bookings</Button>
            <Button variant="outlined" onClick={() => navigate('/invoices')}>Billing</Button>
          </Stack>
        </Stack>
        <Grid container spacing={2}>
          {stats.map((item) => (
            <Grid item xs={12} sm={6} md={3} key={item.label}>
              <Card>
                <CardContent>
                  <Typography color="text.secondary">{item.label}</Typography>
                  <Typography variant="h5">{item.value}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </Box>
  );
}
