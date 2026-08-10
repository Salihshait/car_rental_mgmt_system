import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { getSuggestedPrice, listVehicles } from '../../services/api';

export default function DynamicPricingPage() {
  const [vehicles, setVehicles] = useState([]);
  const [vehicleId, setVehicleId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => { listVehicles().then(setVehicles).catch(() => {}); }, []);

  const handleSuggest = async () => {
    setError('');
    try {
      const data = await getSuggestedPrice({ vehicleId, startDate, endDate });
      setResult(data);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Dynamic Pricing</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }}>
          <TextField select label="Vehicle" value={vehicleId} onChange={(e) => setVehicleId(e.target.value)} sx={{ minWidth: 240 }}>
            {vehicles.map((v) => <MenuItem key={v.id} value={v.id}>{v.registrationNumber}</MenuItem>)}
          </TextField>
          <TextField label="Start Date" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="End Date" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} slotProps={{ inputLabel: { shrink: true } }} />
          <Button variant="contained" onClick={handleSuggest} disabled={!vehicleId || !startDate || !endDate}>Suggest Price</Button>
        </Stack>

        {result ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={4}>
              <Card variant="outlined">
                <CardContent>
                  <Typography color="text.secondary" variant="body2">Base Price</Typography>
                  <Typography variant="h5">{result.basePrice.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={4}>
              <Card variant="outlined">
                <CardContent>
                  <Typography color="text.secondary" variant="body2">Suggested Price</Typography>
                  <Typography variant="h5" color="primary">{result.suggestedPrice.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12}>
              <Stack spacing={1}>
                <Typography variant="subtitle1" fontWeight={600}>Factors</Typography>
                {result.factors.map((f) => (
                  <Stack direction="row" spacing={1} alignItems="center" key={f.name}>
                    <Chip size="small" label={`${f.name}: x${f.multiplier}`} />
                    <Typography variant="body2" color="text.secondary">{f.explanation}</Typography>
                  </Stack>
                ))}
              </Stack>
            </Grid>
          </Grid>
        ) : null}
      </Stack>
    </Box>
  );
}
