import { useState } from 'react';
import { Box, Button, Card, CardContent, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { login } from '../services/api';

export default function AuthPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      const result = await login({ email, password });

      if (result?.accessToken) {
        localStorage.setItem('accessToken', result.accessToken);
      }

      if (result?.refreshToken) {
        localStorage.setItem('refreshToken', result.refreshToken);
      }

      setMessage(result.message ?? 'Login successful.');
      navigate('/dashboard');
    } catch (error) {
      setMessage(error.message ?? 'Unable to reach the API gateway.');
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 3 }}>
      <Card sx={{ width: 420 }}>
        <CardContent sx={{ display: 'grid', gap: 2 }}>
          <Typography variant="h5">Sign In</Typography>
          <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />
          <TextField label="Password" value={password} onChange={(e) => setPassword(e.target.value)} type="password" fullWidth />
          <Button variant="contained" size="large" onClick={handleLogin}>Login</Button>
          {message ? <Typography color="primary.main">{message}</Typography> : null}
        </CardContent>
      </Card>
    </Box>
  );
}
