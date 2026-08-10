import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getPlatformUsageOverview } from '../../services/api';

export default function UsageMonitoringPage() {
  const { data, loading, error, applyFilters } = useReportDashboard(getPlatformUsageOverview);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Usage Monitoring</Typography>

        <DateRangeFilter onApply={applyFilters} />

        {error ? <Alert severity="error">{error}</Alert> : null}
        {loading ? <CircularProgress /> : null}

        {data ? (
          <>
            <Grid container spacing={2}>
              {data.kpis.map((kpi) => (
                <Grid item xs={12} sm={6} md={3} key={kpi.label}>
                  <KpiCard kpi={kpi} />
                </Grid>
              ))}
            </Grid>

            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <ChartCard title="Tenant Growth" type="line" data={data.tenantGrowthTrend} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="MRR Trend" type="line" data={data.mrrTrend} />
              </Grid>
            </Grid>
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
