import { useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getRevenueDashboard, exportRevenueReport } from '../../services/api';

export default function RevenueReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getRevenueDashboard);
  const [drilldown, setDrilldown] = useState({ key: null, value: null });

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Revenue Report</Typography>
          <ExportButtons onExport={(format) => exportRevenueReport(format, filters)} />
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
                <ChartCard title="Revenue Trend" type="line" data={data.trend} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard
                  title="Revenue by Branch"
                  type="bar"
                  data={data.byBranch}
                  onBarClick={(key) => setDrilldown({ key: 'branchName', value: key })}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="Revenue by Vehicle Category" type="bar" data={data.byVehicleCategory} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Bookings"
              columns={[
                { key: 'date', label: 'Date', render: (r) => new Date(r.date).toLocaleDateString() },
                { key: 'branchName', label: 'Branch' },
                { key: 'vehicleRegistrationNumber', label: 'Vehicle' },
                { key: 'status', label: 'Status' },
                {
                  key: 'amount',
                  label: 'Amount',
                  render: (r) => r.amount.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
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
