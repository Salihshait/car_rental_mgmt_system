import { useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getBookingsDashboard, exportBookingsReport } from '../../services/api';

export default function BookingsReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getBookingsDashboard);
  const [drilldown, setDrilldown] = useState({ key: null, value: null });

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Bookings Report</Typography>
          <ExportButtons onExport={(format) => exportBookingsReport(format, filters)} />
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
              <Grid item xs={12}>
                <ChartCard title="Bookings Trend" type="line" data={data.trend} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard
                  title="Bookings by Status"
                  type="bar"
                  data={data.byStatus}
                  onBarClick={(key) => setDrilldown({ key: 'status', value: key })}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard
                  title="Bookings by Branch"
                  type="bar"
                  data={data.byBranch}
                  onBarClick={(key) => setDrilldown({ key: 'branchName', value: key })}
                />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Bookings"
              columns={[
                { key: 'startDate', label: 'Start', render: (r) => new Date(r.startDate).toLocaleDateString() },
                { key: 'endDate', label: 'End', render: (r) => new Date(r.endDate).toLocaleDateString() },
                { key: 'branchName', label: 'Branch' },
                { key: 'vehicleRegistrationNumber', label: 'Vehicle' },
                { key: 'status', label: 'Status' },
                {
                  key: 'totalAmount',
                  label: 'Amount',
                  render: (r) => r.totalAmount.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
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
