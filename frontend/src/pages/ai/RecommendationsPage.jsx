import { useEffect, useState } from 'react';
import { Alert, Box, Card, CardContent, Grid, Stack, Typography } from '@mui/material';
import { getMyRecommendations } from '../../services/api';

export default function RecommendationsPage() {
  const [recommendations, setRecommendations] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    getMyRecommendations().then(setRecommendations).catch((err) => setError(err.message));
  }, []);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Recommended for You</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Grid container spacing={2}>
          {recommendations.map((r) => (
            <Grid item xs={12} sm={6} md={4} key={r.vehicleId}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6">{r.brandName} {r.modelName}</Typography>
                  <Typography color="text.secondary" variant="body2">{r.registrationNumber}</Typography>
                  <Typography variant="subtitle1" sx={{ mt: 1 }}>{r.dailyRate.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}/day</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>{r.reason}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
          {recommendations.length === 0 && !error ? (
            <Grid item xs={12}><Typography color="text.secondary">No recommendations available yet.</Typography></Grid>
          ) : null}
        </Grid>
      </Stack>
    </Box>
  );
}
