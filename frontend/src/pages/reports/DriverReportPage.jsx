import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getDriverReportDashboard, exportDriverReport } from '../../services/api';

export default function DriverReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getDriverReportDashboard);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Driver Report</Typography>
          <ExportButtons onExport={(format) => exportDriverReport(format, filters)} />
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
                <ChartCard title="Rating Distribution" type="bar" data={data.ratingDistribution} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="Attendance Rate Trend" type="line" data={data.attendanceTrend} />
              </Grid>
              <Grid item xs={12}>
                <ChartCard title="Salary Payout by Month" type="bar" data={data.salaryPayoutByMonth} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Drivers"
              columns={[
                { key: 'name', label: 'Name' },
                { key: 'employmentStatus', label: 'Status' },
                { key: 'rating', label: 'Rating', render: (r) => (r.rating != null ? r.rating.toFixed(1) : '-') },
                { key: 'attendanceRate', label: 'Attendance %', render: (r) => `${r.attendanceRate.toFixed(1)}%` },
                {
                  key: 'salaryPaid',
                  label: 'Salary Paid',
                  render: (r) => r.salaryPaid.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
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
