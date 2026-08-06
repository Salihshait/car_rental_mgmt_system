import { useEffect, useRef, useState } from 'react';
import { Box, Button, Card, CardContent, Chip, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { MapContainer, TileLayer, CircleMarker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { getLatestVehicleLocations, listVehicles, simulateTrip } from '../services/api';

const DEFAULT_CENTER = [28.6139, 77.209];

const statusColor = (status) => {
  switch (status) {
    case 'Available':
      return '#2e7d32';
    case 'Rented':
      return '#0288d1';
    case 'InMaintenance':
      return '#ed6c02';
    case 'InTransit':
      return '#9c27b0';
    default:
      return '#757575';
  }
};

export default function LiveMapPage() {
  const [locations, setLocations] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState('');
  const [message, setMessage] = useState('');
  const [simulating, setSimulating] = useState(false);
  const intervalRef = useRef(null);

  const loadLocations = () => {
    getLatestVehicleLocations()
      .then(setLocations)
      .catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listVehicles().then(setVehicles).catch((error) => setMessage(error.message));
    loadLocations();
    intervalRef.current = setInterval(loadLocations, 15000);
    return () => clearInterval(intervalRef.current);
  }, []);

  const handleSimulate = async () => {
    if (!selectedVehicleId) return;
    setMessage('');
    setSimulating(true);
    try {
      await simulateTrip(selectedVehicleId);
      loadLocations();
    } catch (error) {
      setMessage(error.message);
    } finally {
      setSimulating(false);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Live Map</Typography>

        <Card>
          <CardContent>
            <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
              <TextField
                select
                label="Vehicle"
                value={selectedVehicleId}
                onChange={(e) => setSelectedVehicleId(e.target.value)}
                sx={{ minWidth: 240 }}
              >
                <MenuItem value="">Select a vehicle</MenuItem>
                {vehicles.map((vehicle) => (
                  <MenuItem key={vehicle.id} value={vehicle.id}>{vehicle.registrationNumber}</MenuItem>
                ))}
              </TextField>
              <Button variant="contained" onClick={handleSimulate} disabled={!selectedVehicleId || simulating}>
                {simulating ? 'Running...' : 'Run Demo Trip'}
              </Button>
              <Typography variant="body2" color="text.secondary">Refreshes automatically every 15s.</Typography>
            </Stack>
          </CardContent>
        </Card>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Card>
          <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
            <Box sx={{ height: 560, width: '100%' }}>
              <MapContainer center={DEFAULT_CENTER} zoom={6} style={{ height: '100%', width: '100%' }}>
                <TileLayer
                  attribution='&copy; OpenStreetMap contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {locations.map((loc) => (
                  <CircleMarker
                    key={loc.vehicleId}
                    center={[loc.latitude, loc.longitude]}
                    radius={9}
                    pathOptions={{ color: statusColor(loc.fleetAvailabilityStatus), fillColor: statusColor(loc.fleetAvailabilityStatus), fillOpacity: 0.8 }}
                  >
                    <Popup>
                      <Stack spacing={0.5}>
                        <Typography variant="subtitle2">{loc.vehicleRegistrationNumber}</Typography>
                        <Chip label={loc.fleetAvailabilityStatus} size="small" />
                        {loc.activeDriverName ? <Typography variant="body2">Driver: {loc.activeDriverName}</Typography> : null}
                        {loc.speedKmh != null ? <Typography variant="body2">Speed: {loc.speedKmh} km/h</Typography> : null}
                        <Typography variant="caption" color="text.secondary">
                          {new Date(loc.recordedAt).toLocaleString()}
                        </Typography>
                      </Stack>
                    </Popup>
                  </CircleMarker>
                ))}
              </MapContainer>
            </Box>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
