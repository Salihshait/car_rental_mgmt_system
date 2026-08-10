import { useState } from 'react';
import { Alert, Box, Button, Stack, Tab, Table, TableBody, TableCell, TableRow, Tabs, Typography } from '@mui/material';
import { extractDrivingLicense, extractRcBook } from '../../services/api';

const TABS = ['Driving License', 'RC Book'];

export default function DocumentOcrPage() {
  const [tab, setTab] = useState(0);
  const [file, setFile] = useState(null);
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState(false);

  const handleExtract = async () => {
    if (!file) return;
    setUploading(true);
    setError('');
    try {
      const data = tab === 0 ? await extractDrivingLicense(file) : await extractRcBook(file);
      setResult(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setUploading(false);
    }
  };

  const changeTab = (value) => {
    setTab(value);
    setResult(null);
    setFile(null);
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Document OCR</Typography>
        <Alert severity="info">Simulated — no real OCR provider is configured. Extracted fields are plausible placeholders for demoing the workflow.</Alert>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Tabs value={tab} onChange={(_, value) => changeTab(value)}>
          {TABS.map((label) => <Tab key={label} label={label} />)}
        </Tabs>

        <Stack direction="row" spacing={2} alignItems="center">
          <Button variant="outlined" component="label">
            Choose Image
            <input type="file" accept="image/*" hidden onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          </Button>
          {file ? <Typography variant="body2">{file.name}</Typography> : null}
          <Button variant="contained" onClick={handleExtract} disabled={!file || uploading}>Extract</Button>
        </Stack>

        {result ? (
          <Stack spacing={1}>
            <Typography variant="subtitle1" fontWeight={600}>Extracted Fields — Confidence {Math.round(result.confidenceScore * 100)}%</Typography>
            <Table size="small" sx={{ maxWidth: 480 }}>
              <TableBody>
                {Object.entries(result.extractedFields).map(([key, value]) => (
                  <TableRow key={key}>
                    <TableCell sx={{ fontWeight: 600 }}>{key}</TableCell>
                    <TableCell>{value}</TableCell>
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
