import { useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getMaintenanceReportDashboard, exportMaintenanceReport } from '../../services/api';

export default function MaintenanceReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getMaintenanceReportDashboard);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Maintenance Report</Typography>
          <ExportButtons onExport={(format) => exportMaintenanceReport(format, filters)} />
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
                <ChartCard title="Cost by Category" type="bar" data={data.costByCategory} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="Vendor Performance" type="bar" data={data.vendorPerformance} />
              </Grid>
              <Grid item xs={12}>
                <ChartCard title="Open Work Orders by Type" type="bar" data={data.openWorkOrdersByType} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Cost by Vehicle"
              columns={[
                { key: 'registrationNumber', label: 'Vehicle' },
                {
                  key: 'totalCost',
                  label: 'Total Cost',
                  render: (r) => r.totalCost.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
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
