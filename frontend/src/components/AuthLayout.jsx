import { Box, Card, CardContent, Typography } from '@mui/material';

export default function AuthLayout({ title, children }) {
  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 3 }}>
      <Card sx={{ width: 420 }}>
        <CardContent sx={{ display: 'grid', gap: 2 }}>
          <Typography variant="h5">{title}</Typography>
          {children}
        </CardContent>
      </Card>
    </Box>
  );
}
