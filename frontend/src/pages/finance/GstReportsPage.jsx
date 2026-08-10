import { useEffect, useState } from 'react';
import { Alert, Box, CircularProgress, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material';
import KpiCard from '../../components/reports/KpiCard';
import ChartCard from '../../components/reports/ChartCard';
import DetailDrilldownTable from '../../components/reports/DetailDrilldownTable';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getGstReport, exportGstReport, listBranches } from '../../services/api';

export default function GstReportsPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getGstReport);
  const [branches, setBranches] = useState([]);

  useEffect(() => { listBranches().then(setBranches).catch(() => {}); }, []);

  const handleDateApply = (dateFilters) => applyFilters({ ...filters, ...dateFilters });
  const handleBranchChange = (branchId) => applyFilters({ ...filters, branchId });

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>GST Reports</Typography>
          <ExportButtons onExport={(format) => exportGstReport(format, filters)} />
        </Stack>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }}>
          <DateRangeFilter onApply={handleDateApply} />
          <TextField
            select
            size="small"
            label="Branch"
            value={filters.branchId ?? ''}
            onChange={(e) => handleBranchChange(e.target.value || undefined)}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">All Branches</MenuItem>
            {branches.map((b) => <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>)}
          </TextField>
        </Stack>

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

            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <ChartCard title="Tax by Month" type="line" data={data.byMonth} />
              </Grid>
              <Grid item xs={12} md={6}>
                <ChartCard title="Tax by Branch" type="bar" data={data.byBranch} />
              </Grid>
            </Grid>

            <DetailDrilldownTable
              title="Invoices"
              columns={[
                { key: 'invoiceNumber', label: 'Invoice' },
                { key: 'issueDate', label: 'Issue Date', render: (r) => new Date(r.issueDate).toLocaleDateString() },
                { key: 'branchName', label: 'Branch' },
                { key: 'taxableValue', label: 'Taxable Value', render: (r) => r.taxableValue.toLocaleString(undefined, { style: 'currency', currency: 'USD' }) },
                { key: 'cgst', label: 'CGST', render: (r) => r.cgst.toLocaleString(undefined, { style: 'currency', currency: 'USD' }) },
                { key: 'sgst', label: 'SGST', render: (r) => r.sgst.toLocaleString(undefined, { style: 'currency', currency: 'USD' }) },
                { key: 'igst', label: 'IGST', render: (r) => r.igst.toLocaleString(undefined, { style: 'currency', currency: 'USD' }) },
                { key: 'totalAmount', label: 'Total', render: (r) => r.totalAmount.toLocaleString(undefined, { style: 'currency', currency: 'USD' }) },
              ]}
              rows={data.detailRows}
            />
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
