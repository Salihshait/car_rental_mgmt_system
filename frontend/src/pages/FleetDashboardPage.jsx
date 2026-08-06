import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { getFleetDashboardSummary, getFleetAvailability } from '../services/api';

const statusColor = (status) => {
  switch (status) {
    case 'Available':
      return 'success';
    case 'Rented':
      return 'info';
    case 'InMaintenance':
    case 'Maintenance':
      return 'warning';
    case 'InTransit':
      return 'secondary';
    case 'Accident':
      return 'error';
    default:
      return 'default';
  }
};

export default function FleetDashboardPage() {
  const [summary, setSummary] = useState(null);
  const [availability, setAvailability] = useState([]);
  const [message, setMessage] = useState('');

  useEffect(() => {
    Promise.all([getFleetDashboardSummary(), getFleetAvailability()])
      .then(([summaryResult, availabilityResult]) => {
        setSummary(summaryResult);
        setAvailability(availabilityResult);
      })
      .catch((error) => setMessage(error.message));
  }, []);

  const tiles = summary
    ? [
        { label: 'Total Vehicles', value: summary.totalVehicles },
        { label: 'Available', value: summary.availableVehicles },
        { label: 'Utilization', value: `${summary.utilizationPercent}%` },
        { label: 'Active Trips', value: summary.activeTripCount },
        { label: 'Reporting GPS', value: summary.vehiclesReportingGpsCount },
        { label: 'Maintenance Due (7d)', value: summary.maintenanceDueCount },
        { label: 'Maintenance Overdue', value: summary.maintenanceOverdueCount },
        { label: 'Active Driver Assignments', value: summary.activeDriverAssignmentCount },
        { label: 'In-Transit Transfers', value: summary.inTransitTransferCount },
        { label: 'Fuel Cost (this month)', value: summary.fuelCostThisMonth },
      ]
    : [];

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Fleet Dashboard</Typography>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Grid container spacing={2}>
          {tiles.map((tile) => (
            <Grid item xs={12} sm={6} md={3} key={tile.label}>
              <Card>
                <CardContent>
                  <Typography color="text.secondary">{tile.label}</Typography>
                  <Typography variant="h5">{tile.value}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Vehicle Availability</Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Registration</TableCell>
                    <TableCell>Branch</TableCell>
                    <TableCell>Vehicle Status</TableCell>
                    <TableCell>Fleet Availability</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {availability.map((row) => (
                    <TableRow key={row.vehicleId} hover>
                      <TableCell>{row.registrationNumber}</TableCell>
                      <TableCell>{row.branchName}</TableCell>
                      <TableCell>{row.vehicleStatus}</TableCell>
                      <TableCell>
                        <Chip label={row.fleetAvailabilityStatus} color={statusColor(row.fleetAvailabilityStatus)} size="small" />
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
