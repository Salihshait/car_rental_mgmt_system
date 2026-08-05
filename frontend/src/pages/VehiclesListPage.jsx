import { useEffect, useState } from 'react';
import {
  Box,
  Button,
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
import {
  exportVehicles,
  getFleetDashboard,
  importVehicles,
  listVehicleBrands,
  listVehicles,
  listBranches,
} from '../services/api';

const STATUSES = ['Available', 'Booked', 'Maintenance', 'Cleaning', 'On Rent', 'Reserved', 'Accident'];

const statusColor = (status) => {
  switch (status) {
    case 'Available':
      return 'success';
    case 'Accident':
      return 'error';
    case 'Maintenance':
    case 'Cleaning':
      return 'warning';
    default:
      return 'default';
  }
};

export default function VehiclesListPage() {
  const [vehicles, setVehicles] = useState([]);
  const [brands, setBrands] = useState([]);
  const [branches, setBranches] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [filters, setFilters] = useState({ search: '', status: '', brandId: '', branchId: '' });
  const [message, setMessage] = useState('');
  const [importResult, setImportResult] = useState(null);

  const loadStatic = async () => {
    const [brandsResult, branchesResult, dashboardResult] = await Promise.all([
      listVehicleBrands(),
      listBranches(),
      getFleetDashboard(),
    ]);
    setBrands(brandsResult);
    setBranches(branchesResult);
    setDashboard(dashboardResult);
  };

  const loadVehicles = async (activeFilters = filters) => {
    const result = await listVehicles(activeFilters);
    setVehicles(result);
  };

  useEffect(() => {
    loadStatic().catch((error) => setMessage(error.message));
    loadVehicles().catch((error) => setMessage(error.message));
  }, []);

  const handleFilterChange = (field, value) => {
    const next = { ...filters, [field]: value };
    setFilters(next);
    loadVehicles(next).catch((error) => setMessage(error.message));
  };

  const handleImport = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;
    setMessage('');
    try {
      const result = await importVehicles(file);
      setImportResult(result);
      await loadVehicles();
      await loadStatic();
    } catch (error) {
      setMessage(error.message);
    } finally {
      event.target.value = '';
    }
  };

  const handleExport = async () => {
    try {
      await exportVehicles(filters);
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Vehicles</Typography>
          <Stack direction="row" spacing={2}>
            <Button component="label" variant="outlined">
              Import CSV
              <input type="file" accept=".csv" hidden onChange={handleImport} />
            </Button>
            <Button variant="outlined" onClick={handleExport}>Export CSV</Button>
            <Button component={RouterLink} to="/vehicles/new" variant="contained">Add Vehicle</Button>
          </Stack>
        </Stack>

        {dashboard ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Total Fleet</Typography>
                <Typography variant="h5">{dashboard.totalVehicles}</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Available</Typography>
                <Typography variant="h5">{dashboard.availableVehicles}</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Utilization</Typography>
                <Typography variant="h5">{dashboard.utilizationPercent}%</Typography>
              </CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent>
                <Typography color="text.secondary">Revenue</Typography>
                <Typography variant="h5">{dashboard.revenue}</Typography>
              </CardContent></Card>
            </Grid>
          </Grid>
        ) : null}

        <Card>
          <CardContent>
            <Stack direction="row" spacing={2} flexWrap="wrap">
              <TextField
                label="Search (reg. no / VIN / engine no)"
                value={filters.search}
                onChange={(e) => handleFilterChange('search', e.target.value)}
                sx={{ minWidth: 260 }}
              />
              <TextField select label="Status" value={filters.status} onChange={(e) => handleFilterChange('status', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                {STATUSES.map((status) => (
                  <MenuItem key={status} value={status}>{status}</MenuItem>
                ))}
              </TextField>
              <TextField select label="Brand" value={filters.brandId} onChange={(e) => handleFilterChange('brandId', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                {brands.map((brand) => (
                  <MenuItem key={brand.id} value={brand.id}>{brand.name}</MenuItem>
                ))}
              </TextField>
              <TextField select label="Branch" value={filters.branchId} onChange={(e) => handleFilterChange('branchId', e.target.value)} sx={{ minWidth: 160 }}>
                <MenuItem value="">All</MenuItem>
                {branches.map((branch) => (
                  <MenuItem key={branch.id} value={branch.id}>{branch.name}</MenuItem>
                ))}
              </TextField>
            </Stack>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}
        {importResult ? (
          <Typography color="primary.main">
            Import: {importResult.successCount} succeeded, {importResult.failureCount} failed out of {importResult.totalRows} rows.
          </Typography>
        ) : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Registration</TableCell>
                    <TableCell>Brand / Model</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Branch</TableCell>
                    <TableCell>Daily Rate</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {vehicles.map((vehicle) => (
                    <TableRow key={vehicle.id} hover>
                      <TableCell>
                        <RouterLink to={`/vehicles/${vehicle.id}`}>{vehicle.registrationNumber}</RouterLink>
                      </TableCell>
                      <TableCell>{[vehicle.brandName, vehicle.modelName].filter(Boolean).join(' ') || '-'}</TableCell>
                      <TableCell>
                        <Chip label={vehicle.status} color={statusColor(vehicle.status)} size="small" />
                      </TableCell>
                      <TableCell>{vehicle.branchName}</TableCell>
                      <TableCell>{vehicle.dailyRate}</TableCell>
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
