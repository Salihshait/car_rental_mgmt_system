import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const SECTIONS = [
  { label: 'Income', to: '/finance/income', description: 'Booking revenue and other income, trended and by category.' },
  { label: 'Expenses', to: '/finance/expenses', description: 'Maintenance, driver salaries, refunds, and manual expenses.' },
  { label: 'Cashbook', to: '/finance/cashbook', description: 'Chronological cash movements with a running balance.' },
  { label: 'Bank', to: '/finance/bank', description: 'Bank accounts and their transaction ledgers.' },
  { label: 'Journal', to: '/finance/journal', description: 'Manual income/expense entries not captured elsewhere.' },
  { label: 'Ledger', to: '/finance/ledger', description: 'All transactions grouped by account/category.' },
  { label: 'Profit & Loss', to: '/finance/profit-loss', description: 'Income vs expenses statement for a period.' },
  { label: 'Balance Sheet', to: '/finance/balance-sheet', description: 'Assets, liabilities, and equity as of a date (simplified snapshot).' },
  { label: 'GST Reports', to: '/finance/gst', description: 'CGST/SGST/IGST summary by month and branch.' },
];

export default function FinanceHubPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Finance</Typography>
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
