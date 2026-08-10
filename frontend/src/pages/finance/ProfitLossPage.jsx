import { Alert, Box, CircularProgress, Grid, Stack, Table, TableBody, TableCell, TableRow, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getProfitLoss, exportProfitLoss } from '../../services/api';

const currency = (value) => value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });

function StatementTable({ title, rows, total }) {
  return (
    <Box>
      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>{title}</Typography>
      <Table size="small">
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.key}>
              <TableCell>{row.key}</TableCell>
              <TableCell align="right">{currency(row.value)}</TableCell>
            </TableRow>
          ))}
          <TableRow>
            <TableCell sx={{ fontWeight: 700 }}>Total</TableCell>
            <TableCell align="right" sx={{ fontWeight: 700 }}>{currency(total)}</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </Box>
  );
}

export default function ProfitLossPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getProfitLoss);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Profit &amp; Loss</Typography>
          <ExportButtons onExport={(format) => exportProfitLoss(format, filters)} />
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
                <StatementTable
                  title="Income"
                  rows={data.incomeByCategory.map((p) => ({ key: p.key, value: p.value }))}
                  total={data.incomeByCategory.reduce((sum, p) => sum + p.value, 0)}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <StatementTable
                  title="Expenses"
                  rows={data.expenseByCategory.map((p) => ({ key: p.key, value: p.value }))}
                  total={data.expenseByCategory.reduce((sum, p) => sum + p.value, 0)}
                />
              </Grid>
            </Grid>

            <ChartCard title="Monthly Net" type="line" data={data.monthlyTrend} />
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
