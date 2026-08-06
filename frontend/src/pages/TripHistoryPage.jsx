import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Chip,
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
import { MapContainer, TileLayer, Polyline } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { getTripLocations, listTrips, listVehicles } from '../services/api';

export default function TripHistoryPage() {
  const [trips, setTrips] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [vehicleId, setVehicleId] = useState('');
  const [selectedTrip, setSelectedTrip] = useState(null);
  const [polyline, setPolyline] = useState([]);
  const [message, setMessage] = useState('');

  const loadTrips = (filters = {}) => {
    listTrips(filters).then(setTrips).catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    loadTrips();
  }, []);

  const handleVehicleFilter = (value) => {
    setVehicleId(value);
    loadTrips(value ? { vehicleId: value } : {});
  };

  const handleSelectTrip = async (trip) => {
    setSelectedTrip(trip);
    try {
      const locations = await getTripLocations(trip.id);
      setPolyline(locations.map((loc) => [loc.latitude, loc.longitude]));
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Trip History</Typography>

        <Card>
          <CardContent>
            <TextField
              select
              label="Vehicle"
              value={vehicleId}
              onChange={(e) => handleVehicleFilter(e.target.value)}
              sx={{ minWidth: 240 }}
            >
              <MenuItem value="">All vehicles</MenuItem>
              {vehicles.map((vehicle) => (
                <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
              ))}
            </TextField>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Vehicle</TableCell>
                    <TableCell>Driver</TableCell>
                    <TableCell>Started</TableCell>
                    <TableCell>Ended</TableCell>
                    <TableCell>Distance (km)</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {trips.map((trip) => (
                    <TableRow key={trip.id} hover selected={selectedTrip?.id === trip.id} onClick={() => handleSelectTrip(trip)} sx={{ cursor: 'pointer' }}>
                      <TableCell>{trip.vehicleRegistrationNumber}</TableCell>
                      <TableCell>{trip.driverName ?? '-'}</TableCell>
                      <TableCell>{new Date(trip.startedAt).toLocaleString()}</TableCell>
                      <TableCell>{trip.endedAt ? new Date(trip.endedAt).toLocaleString() : '-'}</TableCell>
                      <TableCell>{trip.distanceKm}</TableCell>
                      <TableCell><Chip label={trip.status} size="small" color={trip.status === 'InProgress' ? 'info' : 'default'} /></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        {selectedTrip ? (
          <Card>
            <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
              <Box sx={{ height: 420, width: '100%' }}>
                <MapContainer
                  center={polyline[0] ?? [selectedTrip.startLatitude, selectedTrip.startLongitude]}
                  zoom={12}
                  style={{ height: '100%', width: '100%' }}
                >
                  <TileLayer attribution='&copy; OpenStreetMap contributors' url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
                  {polyline.length > 1 ? <Polyline positions={polyline} pathOptions={{ color: '#0d6efd' }} /> : null}
                </MapContainer>
              </Box>
            </CardContent>
          </Card>
        ) : null}
      </Stack>
    </Box>
  );
}
