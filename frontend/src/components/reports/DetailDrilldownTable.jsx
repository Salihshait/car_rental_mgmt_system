import { useMemo } from 'react';
import {
  Card,
  CardContent,
  Chip,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';

export default function DetailDrilldownTable({ title, columns, rows, filterKey, filterValue, onClearFilter }) {
  const filteredRows = useMemo(() => {
    if (!filterKey || !filterValue) return rows;
    return rows.filter((row) => String(row[filterKey]) === String(filterValue));
  }, [rows, filterKey, filterValue]);

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
          <Typography variant="subtitle1" fontWeight={600}>{title}</Typography>
          {filterValue ? <Chip size="small" label={`Filtered: ${filterValue}`} onDelete={onClearFilter} /> : null}
        </Stack>
        <TableContainer sx={{ maxHeight: 360 }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((col) => (
                  <TableCell key={col.key}>{col.label}</TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredRows.slice(0, 200).map((row, index) => (
                <TableRow key={row.id ?? index} hover>
                  {columns.map((col) => (
                    <TableCell key={col.key}>{col.render ? col.render(row) : row[col.key]}</TableCell>
                  ))}
                </TableRow>
              ))}
              {filteredRows.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={columns.length}>
                    <Typography color="text.secondary" variant="body2">No records found.</Typography>
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );
}
