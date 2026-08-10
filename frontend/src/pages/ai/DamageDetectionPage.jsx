import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
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
import { analyzeDamage, listDamageDetectionHistory } from '../../services/api';

export default function DamageDetectionPage() {
  const [file, setFile] = useState(null);
  const [result, setResult] = useState(null);
  const [history, setHistory] = useState([]);
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState(false);

  const load = () => listDamageDetectionHistory().then(setHistory).catch((err) => setError(err.message));
  useEffect(load, []);

  const handleAnalyze = async () => {
    if (!file) return;
    setUploading(true);
    setError('');
    try {
      const data = await analyzeDamage({ image: file });
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
        <Typography variant="h4" fontWeight={700}>Vehicle Damage Detection</Typography>
        <Alert severity="info">Simulated — no real computer-vision provider is configured. Results are deterministic placeholders for demoing the workflow.</Alert>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction="row" spacing={2} alignItems="center">
          <Button variant="outlined" component="label">
            Choose Image
            <input type="file" accept="image/*" hidden onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          </Button>
          {file ? <Typography variant="body2">{file.name}</Typography> : null}
          <Button variant="contained" onClick={handleAnalyze} disabled={!file || uploading}>Analyze</Button>
        </Stack>

        {result ? (
          <Stack spacing={1}>
            <Typography variant="subtitle1" fontWeight={600}>Result — Severity {result.severityScore}</Typography>
            {result.detectedDamages.length === 0 ? (
              <Typography color="text.secondary" variant="body2">No damage detected.</Typography>
            ) : (
              <Stack direction="row" spacing={1} flexWrap="wrap">
                {result.detectedDamages.map((d, i) => (
                  <Chip key={i} label={`${d.damageType} @ ${d.location} (${Math.round(d.confidence * 100)}%)`} color="warning" />
                ))}
              </Stack>
            )}
          </Stack>
        ) : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Image</TableCell>
                <TableCell>Damages</TableCell>
                <TableCell>Severity</TableCell>
                <TableCell>Date</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {history.map((h) => (
                <TableRow key={h.id}>
                  <TableCell>{h.imageReference}</TableCell>
                  <TableCell>{h.detectedDamages.map((d) => d.damageType).join(', ') || '-'}</TableCell>
                  <TableCell>{h.severityScore}</TableCell>
                  <TableCell>{new Date(h.createdAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>
    </Box>
  );
}
