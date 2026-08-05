import { useEffect, useState } from 'react';
import { Box, Button, Card, CardContent, Chip, Stack, Typography } from '@mui/material';
import { listMyNotifications, markNotificationRead } from '../services/api';

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState([]);
  const [message, setMessage] = useState('');

  const load = () => listMyNotifications().then(setNotifications).catch((error) => setMessage(error.message));

  useEffect(() => {
    load();
  }, []);

  const handleMarkRead = async (id) => {
    try {
      await markNotificationRead(id);
      setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 700 }}>
        <Typography variant="h4" fontWeight={700}>Notifications</Typography>
        {message ? <Typography color="error">{message}</Typography> : null}
        <Card>
          <CardContent>
            <Stack spacing={2}>
              {notifications.map((n) => (
                <Stack key={n.id} direction="row" spacing={2} alignItems="center">
                  <Chip label={n.notificationType} size="small" />
                  <Typography variant="body2" sx={{ flexGrow: 1 }}>{n.message}</Typography>
                  <Typography variant="caption" color="text.secondary">{new Date(n.createdAt).toLocaleString()}</Typography>
                  {!n.isRead && <Button size="small" onClick={() => handleMarkRead(n.id)}>Mark Read</Button>}
                </Stack>
              ))}
              {notifications.length === 0 && <Typography color="text.secondary">No notifications.</Typography>}
            </Stack>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
