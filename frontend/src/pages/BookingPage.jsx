import { Box, Button, Card, CardContent, Stack, TextField, Typography } from '@mui/material';

export default function BookingPage() {
  return (
    <Box sx={{ p: 4 }}>
      <Card>
        <CardContent>
          <Stack spacing={2}>
            <Typography variant="h5">Booking Wizard</Typography>
            <TextField label="Start Date" type="date" fullWidth />
            <TextField label="End Date" type="date" fullWidth />
            <TextField label="Vehicle Category" fullWidth />
            <Button variant="contained">Search Cars</Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
