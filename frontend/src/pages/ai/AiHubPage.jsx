import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const SECTIONS = [
  { label: 'Dynamic Pricing', to: '/ai/pricing', description: 'Suggested price for a vehicle and date range, with a factor breakdown.' },
  { label: 'Demand Forecast', to: '/ai/demand', description: 'Historical booking trend projected forward.' },
  { label: 'Predictive Maintenance', to: '/ai/maintenance-predictions', description: 'Vehicles likely to need service soon, based on time and mileage.' },
  { label: 'Fraud Alerts', to: '/ai/fraud', description: 'Bookings flagged by risk signals for staff review.' },
  { label: 'Damage Detection', to: '/ai/damage-detection', description: 'Upload a vehicle photo for a simulated damage report.' },
  { label: 'Document OCR', to: '/ai/ocr', description: 'Extract fields from a driving license or RC book photo (simulated).' },
  { label: 'Revenue Forecast', to: '/ai/revenue-forecast', description: 'Historical revenue trend projected forward.' },
  { label: 'Recommendations', to: '/ai/recommendations', description: 'Vehicles recommended for you, with a reason.' },
  { label: 'Voice Booking', to: '/ai/voice-booking', description: 'Submit an audio clip and review a simulated transcript + parsed intent.' },
];

export default function AiHubPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>AI</Typography>
        <Typography color="text.secondary">
          Pricing, demand, maintenance, fraud, revenue, and recommendations run on real algorithms over this app's own data.
          Damage detection, OCR, and voice transcription are simulated — no vision/speech provider is configured. The chatbot is available from the widget in the corner of every page.
        </Typography>
        <Grid container spacing={2}>
          {SECTIONS.map((section) => (
            <Grid item xs={12} sm={6} md={4} key={section.to}>
              <Card variant="outlined">
                <CardActionArea onClick={() => navigate(section.to)} sx={{ height: '100%' }}>
                  <CardContent>
                    <Typography variant="h6">{section.label}</Typography>
                    <Typography color="text.secondary" variant="body2">{section.description}</Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </Box>
  );
}
