import { useEffect, useState } from 'react';
import {
  Box,
  Button,
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
import { generateInvoice, listBookings, listInvoices } from '../services/api';

export default function InvoicesPage() {
  const [bookings, setBookings] = useState([]);
  const [invoices, setInvoices] = useState([]);
  const [message, setMessage] = useState('');
  const [generatingId, setGeneratingId] = useState(null);

  const loadData = async () => {
    try {
      const [bookingsResult, invoicesResult] = await Promise.all([listBookings(), listInvoices()]);
      setBookings(bookingsResult);
      setInvoices(invoicesResult);
    } catch (error) {
      setMessage(error.message ?? 'Unable to load billing data.');
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleGenerate = async (bookingId) => {
    setGeneratingId(bookingId);
    setMessage('');
    try {
      await generateInvoice(bookingId);
      await loadData();
    } catch (error) {
      setMessage(error.message ?? 'Unable to generate invoice.');
    } finally {
      setGeneratingId(null);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Card>
        <CardContent>
          <Stack spacing={2}>
            <Typography variant="h5">Billing</Typography>
            {message ? <Typography color="error">{message}</Typography> : null}
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Booking</TableCell>
                    <TableCell>Dates</TableCell>
                    <TableCell>Total Amount</TableCell>
                    <TableCell>Invoice Number</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Action</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {bookings.map((booking) => {
                    const invoice = invoices.find((inv) => inv.bookingId === booking.id);
                    return (
                      <TableRow key={booking.id}>
                        <TableCell>{booking.id}</TableCell>
                        <TableCell>
                          {new Date(booking.startDate).toLocaleDateString()} -{' '}
                          {new Date(booking.endDate).toLocaleDateString()}
                        </TableCell>
                        <TableCell>{booking.totalAmount}</TableCell>
                        <TableCell>{invoice?.invoiceNumber ?? '-'}</TableCell>
                        <TableCell>
                          {invoice ? (
                            <Chip
                              label={invoice.status}
                              color={invoice.status === 'Paid' ? 'success' : 'warning'}
                              size="small"
                            />
                          ) : (
                            '-'
                          )}
                        </TableCell>
                        <TableCell align="right">
                          {!invoice ? (
                            <Button
                              variant="contained"
                              size="small"
                              disabled={generatingId === booking.id}
                              onClick={() => handleGenerate(booking.id)}
                            >
                              Generate Invoice
                            </Button>
                          ) : null}
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
