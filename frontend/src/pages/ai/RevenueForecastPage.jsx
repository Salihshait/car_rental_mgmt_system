import { useEffect, useState } from 'react';
import { Alert, Box, CircularProgress, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import { getRevenueForecast } from '../../services/api';

export default function RevenueForecastPage() {
  const [monthsAhead, setMonthsAhead] = useState(3);
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = () => {
    setLoading(true);
    setError('');
    getRevenueForecast({ monthsAhead })
      .then(setData)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(load, [monthsAhead]);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Revenue Forecast</Typography>

        <TextField
          select
          size="small"
          label="Months Ahead"
          value={monthsAhead}
          onChange={(e) => setMonthsAhead(Number(e.target.value))}
          sx={{ maxWidth: 200 }}
        >
          {[1, 2, 3, 6].map((m) => <MenuItem key={m} value={m}>{m}</MenuItem>)}
        </TextField>

        {error ? <Alert severity="error">{error}</Alert> : null}
        {loading ? <CircularProgress /> : null}

        {data ? (
          <>
            <Grid container spacing={2}>
              {data.kpis.map((kpi) => (
                <Grid item xs={12} sm={6} md={4} key={kpi.label}>
                  <KpiCard kpi={kpi} />
                </Grid>
              ))}
            </Grid>
            <ChartCard title="Revenue: History + Forecast" type="line" data={data.trend} />
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
