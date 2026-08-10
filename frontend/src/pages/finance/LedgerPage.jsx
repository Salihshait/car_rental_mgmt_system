import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Chip,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import DateRangeFilter from '../../components/reports/DateRangeFilter';
import ExportButtons from '../../components/reports/ExportButtons';
import { useReportDashboard } from '../../components/reports/useReportDashboard';
import { getLedger, exportLedger } from '../../services/api';

const currency = (value) => value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });

export default function LedgerPage() {
  const { data, loading, error, filters, applyFilters } = useReportDashboard(getLedger);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
          <Typography variant="h4" fontWeight={700}>Ledger</Typography>
          <ExportButtons onExport={(format) => exportLedger(format, filters)} />
        </Stack>

        <DateRangeFilter onApply={applyFilters} />

        {error ? <Alert severity="error">{error}</Alert> : null}
        {loading ? <CircularProgress /> : null}

        {data ? (
          <Stack spacing={1}>
            {data.map((account) => (
              <Accordion key={account.account} disableGutters>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Stack direction="row" spacing={2} alignItems="center" sx={{ flexGrow: 1 }}>
                    <Typography sx={{ flexGrow: 1 }}>{account.account}</Typography>
                    <Chip size="small" label={`Net ${currency(account.net)}`} color={account.net >= 0 ? 'success' : 'error'} />
                  </Stack>
                </AccordionSummary>
                <AccordionDetails>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Date</TableCell>
                        <TableCell>Description</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell align="right">Amount</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {account.entries.map((entry, index) => (
                        <TableRow key={index}>
                          <TableCell>{new Date(entry.date).toLocaleDateString()}</TableCell>
                          <TableCell>{entry.description}</TableCell>
                          <TableCell>{entry.type}</TableCell>
                          <TableCell align="right">{currency(entry.amount)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </AccordionDetails>
              </Accordion>
            ))}
            {data.length === 0 ? <Typography color="text.secondary">No accounts for this period.</Typography> : null}
          </Stack>
        ) : null}
      </Stack>
    </Box>
  );
}
