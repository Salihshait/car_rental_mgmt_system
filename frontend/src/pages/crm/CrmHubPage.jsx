import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const SECTIONS = [
  { label: 'Support Tickets', to: '/crm/support', description: 'Customer support queue — assign, reply, and resolve tickets.' },
  { label: 'Complaints', to: '/crm/complaints', description: 'Formal complaints tied to bookings or vehicles, with resolution tracking.' },
  { label: 'Feedback', to: '/crm/feedback', description: 'Post-booking ratings and comments — review and publish.' },
  { label: 'Message Templates', to: '/crm/templates', description: 'Reusable Email/SMS/WhatsApp/Push templates with placeholders.' },
  { label: 'Campaigns', to: '/crm/campaigns', description: 'Bulk-send a template to a filtered customer audience.' },
  { label: 'Message Logs', to: '/crm/messages', description: 'Email, SMS, WhatsApp, and Push activity — and ad-hoc sends.' },
];

export default function CrmHubPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>CRM</Typography>
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
