import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { getCustomerDashboard, listCustomers } from '../services/api';

const KYC_STATUSES = ['Pending', 'Verified', 'Rejected'];

const kycColor = (status) => {
  switch (status) {
    case 'Verified':
      return 'success';
    case 'Rejected':
      return 'error';
    default:
      return 'warning';
  }
};

export default function CustomersListPage() {
  const [customers, setCustomers] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [filters, setFilters] = useState({ search: '', kycStatus: '', isBlacklisted: '', isCorporate: '' });
  const [message, setMessage] = useState('');

  const loadCustomers = async (activeFilters = filters) => {
    const result = await listCustomers(activeFilters);
    setCustomers(result);
  };

  useEffect(() => {
    getCustomerDashboard().then(setDashboard).catch((error) => setMessage(error.message));
    loadCustomers().catch((error) => setMessage(error.message));
  }, []);

  const handleFilterChange = (field, value) => {
    const next = { ...filters, [field]: value };
    setFilters(next);
    loadCustomers(next).catch((error) => setMessage(error.message));
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Customers</Typography>

        {dashboard ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Total Customers</Typography>
                <Typography variant="h5">{dashboard.totalCustomers}</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Blacklisted</Typography>
                <Typography variant="h5">{dashboard.blacklistedCount}</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Corporate</Typography>
                <Typography variant="h5">{dashboard.corporateCount}</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Total Wallet Balance</Typography>
                <Typography variant="h5">{dashboard.totalWalletBalance}</Typography>
              </CardContent></Card>
            </Grid>
          </Grid>
        ) : null}

        <Card>
          <CardContent>
            <Stack direction="row" spacing={2} flexWrap="wrap">
              <TextField
                label="Search (name / email)"
                value={filters.search}
                onChange={(e) => handleFilterChange('search', e.target.value)}
                sx={{ minWidth: 260 }}
              />
              <TextField select label="KYC Status" value={filters.kycStatus} onChange={(e) => handleFilterChange('kycStatus', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                {KYC_STATUSES.map((status) => <MenuItem key={status} value={status}>{status}</MenuItem>)}
              </TextField>
              <TextField select label="Blacklisted" value={filters.isBlacklisted} onChange={(e) => handleFilterChange('isBlacklisted', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                <MenuItem value="true">Blacklisted</MenuItem>
                <MenuItem value="false">Not Blacklisted</MenuItem>
              </TextField>
              <TextField select label="Corporate" value={filters.isCorporate} onChange={(e) => handleFilterChange('isCorporate', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                <MenuItem value="true">Corporate</MenuItem>
                <MenuItem value="false">Individual</MenuItem>
              </TextField>
            </Stack>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>KYC</TableCell>
                    <TableCell>Wallet</TableCell>
                    <TableCell>Loyalty</TableCell>
                    <TableCell>Type</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {customers.map((customer) => (
                    <TableRow key={customer.id} hover>
                      <TableCell>
                        <RouterLink to={`/customers/${customer.id}`}>{customer.firstName} {customer.lastName}</RouterLink>
                      </TableCell>
                      <TableCell>{customer.email}</TableCell>
                      <TableCell><Chip label={customer.kycStatus} color={kycColor(customer.kycStatus)} size="small" /></TableCell>
                      <TableCell>{customer.walletBalance}</TableCell>
                      <TableCell>{customer.loyaltyPoints}</TableCell>
                      <TableCell>
                        {customer.isBlacklisted ? <Chip label="Blacklisted" color="error" size="small" sx={{ mr: 1 }} /> : null}
                        {customer.isCorporate ? <Chip label="Corporate" size="small" /> : null}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
