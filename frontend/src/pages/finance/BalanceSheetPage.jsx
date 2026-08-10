import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Grid,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getBalanceSheet, exportBalanceSheet } from '../../services/api';

const currency = (value) => value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });

function StatementColumn({ title, lines, total }) {
  return (
    <Box>
      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>{title}</Typography>
      <Table size="small">
        <TableBody>
          {lines.map((line) => (
            <TableRow key={line.label}>
              <TableCell>{line.label}</TableCell>
              <TableCell align="right">{currency(line.amount)}</TableCell>
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

export default function BalanceSheetPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getBalanceSheet);
  const [asOfDate, setAsOfDate] = useState('');

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Balance Sheet</Typography>
          <ExportButtons onExport={(format) => exportBalanceSheet(format, filters)} />
        </Stack>

        <Stack direction="row" spacing={2} alignItems="center">
          <TextField
            label="As of"
            type="date"
            size="small"
            value={asOfDate}
            onChange={(e) => setAsOfDate(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <Button variant="contained" size="small" onClick={() => applyFilters({ asOfDate })}>Apply</Button>
        </Stack>

        <Alert severity="info">
          This is a simplified snapshot built from existing transaction data, not a full double-entry statement — it is not guaranteed to balance.
        </Alert>

        {error ? <Alert severity="error">{error}</Alert> : null}
        {loading ? <CircularProgress /> : null}

        {data ? (
          <>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <StatementColumn title="Assets" lines={data.assets} total={data.totalAssets} />
              </Grid>
              <Grid item xs={12} md={4}>
                <StatementColumn title="Liabilities" lines={data.liabilities} total={data.totalLiabilities} />
              </Grid>
              <Grid item xs={12} md={4}>
                <StatementColumn title="Equity" lines={data.equity} total={data.totalEquity} />
              </Grid>
            </Grid>

            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="body2" color="text.secondary">Difference (Assets − Liabilities − Equity):</Typography>
              <Chip
                label={currency(data.difference)}
                color={Math.abs(data.difference) < 0.01 ? 'success' : 'warning'}
                size="small"
              />
            </Stack>
          </>
        ) : null}
      </Stack>
    </Box>
  );
}
