import { useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getFleetReportDashboard, exportFleetReport } from '../../services/api';

export default function FleetReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getFleetReportDashboard);
  const [drilldown, setDrilldown] = useState({ key: null, value: null });

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Fleet Report</Typography>
          <ExportButtons onExport={(format) => exportFleetReport(format, filters)} />
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
                <ChartCard
                  title="Status Breakdown"
                  type="bar"
                  data={data.statusBreakdown}
                  onBarClick={(key) => setDrilldown({ key: 'status', value: key })}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="Utilization Trend" type="line" data={data.utilizationTrend} />
              </Grid>
              <Grid item xs={12}>
                <ChartCard title="Revenue by Category" type="bar" data={data.revenueByCategory} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Vehicles"
              columns={[
                { key: 'registrationNumber', label: 'Registration' },
                { key: 'branchName', label: 'Branch' },
                { key: 'status', label: 'Status' },
                { key: 'bookingCount', label: 'Bookings' },
                {
                  key: 'revenueGenerated',
                  label: 'Revenue',
                  render: (r) => r.revenueGenerated.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
                },
              ]}
              rows={data.detailRows}
              filterKey={drilldown.key}
              filterValue={drilldown.value}
              onClearFilter={() => setDrilldown({ key: null, value: null })}
            />
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
