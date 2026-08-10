import { useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getCustomerReportDashboard, exportCustomerReport } from '../../services/api';

export default function CustomerReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getCustomerReportDashboard);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Customer Report</Typography>
          <ExportButtons onExport={(format) => exportCustomerReport(format, filters)} />
        </Stack>

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
                <ChartCard title="New Customers Trend" type="line" data={data.newCustomersTrend} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="By Type" type="bar" data={data.byType} />
              </Grid>
              <Grid item xs={12}>
                <ChartCard title="Top Customers by Spend" type="bar" data={data.topCustomersBySpend} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Customers"
              columns={[
                { key: 'name', label: 'Name' },
                { key: 'email', label: 'Email' },
                { key: 'isCorporate', label: 'Corporate', render: (r) => (r.isCorporate ? 'Yes' : 'No') },
                { key: 'isBlacklisted', label: 'Blacklisted', render: (r) => (r.isBlacklisted ? 'Yes' : 'No') },
                { key: 'bookingCount', label: 'Bookings' },
                {
                  key: 'totalSpend',
                  label: 'Total Spend',
                  render: (r) => r.totalSpend.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
                },
              ]}
              rows={data.detailRows}
            />
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
