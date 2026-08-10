import { useMemo, useState } from 'react';
import { Alert, Box, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard, { CHART_COLORS } from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getFinanceReportDashboard, exportFinanceReport } from '../../services/api';

function mergeTrends(revenueTrend, expensesTrend) {
  const map = new Map();
  revenueTrend.forEach((p) => map.set(p.key, { key: p.key, revenue: p.value, expenses: 0 }));
  expensesTrend.forEach((p) => {
    const existing = map.get(p.key) ?? { key: p.key, revenue: 0, expenses: 0 };
    existing.expenses = p.value;
    map.set(p.key, existing);
  });
  return Array.from(map.values()).sort((a, b) => new Date(`1 ${a.key}`) - new Date(`1 ${b.key}`));
}

const TREND_SERIES = [
  { dataKey: 'revenue', name: 'Revenue', color: CHART_COLORS[0] },
  { dataKey: 'expenses', name: 'Expenses', color: CHART_COLORS[1] },
];

export default function FinanceReportPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getFinanceReportDashboard);
  const [drilldown, setDrilldown] = useState({ key: null, value: null });

  const trend = useMemo(
    () => (data ? mergeTrends(data.revenueTrend, data.expensesTrend) : []),
    [data],
  );

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Finance Report</Typography>
          <ExportButtons onExport={(format) => exportFinanceReport(format, filters)} />
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
                <ChartCard title="Revenue vs Expenses" type="line" data={trend} series={TREND_SERIES} />
              </Grid>
              <Grid item xs={12}>
                <ChartCard title="Payment Methods" type="bar" data={data.paymentMethods} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Outstanding Invoices"
              columns={[
                { key: 'invoiceNumber', label: 'Invoice' },
                { key: 'issueDate', label: 'Issue Date', render: (r) => new Date(r.issueDate).toLocaleDateString() },
                {
                  key: 'totalAmount',
                  label: 'Total',
                  render: (r) => r.totalAmount.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
                },
                {
                  key: 'amountDue',
                  label: 'Due',
                  render: (r) => r.amountDue.toLocaleString(undefined, { style: 'currency', currency: 'USD' }),
                },
                { key: 'daysOutstanding', label: 'Days Outstanding' },
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
