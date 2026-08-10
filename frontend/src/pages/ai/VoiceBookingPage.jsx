import { useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Link,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableRow,
  Typography,
} from '@mui/material';
import { listMyVoiceBookings, submitVoiceBooking } from '../../services/api';

export default function VoiceBookingPage() {
  const [file, setFile] = useState(null);
  const [result, setResult] = useState(null);
  const [history, setHistory] = useState([]);
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState(false);

  const load = () => listMyVoiceBookings().then(setHistory).catch((err) => setError(err.message));
  useEffect(load, []);

  const handleSubmit = async () => {
    if (!file) return;
    setUploading(true);
    setError('');
    try {
      const data = await submitVoiceBooking(file);
      setResult(data);
      load();
    } catch (err) {
      setError(err.message);
    } finally {
      setUploading(false);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Voice Booking</Typography>
        <Alert severity="info">Simulated — no real speech-to-text provider is configured. Any audio file you upload produces the same fixed sample transcript so the workflow can be reviewed end-to-end.</Alert>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction="row" spacing={2} alignItems="center">
          <Button variant="outlined" component="label">
            Choose Audio
            <input type="file" accept="audio/*" hidden onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          </Button>
          {file ? <Typography variant="body2">{file.name}</Typography> : null}
          <Button variant="contained" onClick={handleSubmit} disabled={!file || uploading}>Submit</Button>
        </Stack>

        {result ? (
          <Stack spacing={1}>
            <Typography variant="subtitle1" fontWeight={600}>Transcript</Typography>
            <Typography variant="body2" sx={{ p: 1.5, bgcolor: 'action.hover', borderRadius: 1 }}>{result.transcribedText}</Typography>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mt: 1 }}>Parsed Intent</Typography>
            <Table size="small" sx={{ maxWidth: 480 }}>
              <TableBody>
                {Object.entries(result.parsedIntent).map(([key, value]) => (
                  <TableRow key={key}>
                    <TableCell sx={{ fontWeight: 600 }}>{key}</TableCell>
                    <TableCell>{value}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            <Typography variant="body2">
              Review the details above, then head to <Link component={RouterLink} to="/booking">Bookings</Link> to confirm a real reservation.
            </Typography>
          </Stack>
        ) : null}

        {history.length > 0 ? (
          <Stack spacing={1}>
            <Typography variant="subtitle1" fontWeight={600}>Past Requests</Typography>
            <Table size="small">
              <TableBody>
                {history.map((h) => (
                  <TableRow key={h.id}>
                    <TableCell>{new Date(h.createdAt).toLocaleString()}</TableCell>
                    <TableCell sx={{ maxWidth: 360 }}>{h.transcribedText}</TableCell>
                    <TableCell>{h.status}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Stack>
        ) : null}
      </Stack>
    </Box>
  );
}
