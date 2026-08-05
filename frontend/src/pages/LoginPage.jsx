import { useState } from 'react';
import { Box, Button, Link, Stack, TextField, Typography } from '@mui/material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { supabase } from '../services/supabaseClient';
import { getMyProfile } from '../services/api';
import AuthLayout from '../components/AuthLayout';

export default function LoginPage() {
  const [mode, setMode] = useState('password');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [otpCode, setOtpCode] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const afterSignIn = async () => {
    try {
      await getMyProfile();
    } catch {
      navigate('/complete-profile');
      return;
    }
    navigate('/dashboard');
  };

  const handlePasswordLogin = async () => {
    setMessage('');
    const { error } = await supabase.auth.signInWithPassword({ email, password });
    if (error) {
      setMessage(error.message);
      return;
    }
    await afterSignIn();
  };

  const handleSendOtp = async () => {
    setMessage('');
    const { error } = await supabase.auth.signInWithOtp({ email });
    if (error) {
      setMessage(error.message);
      return;
    }
    setMode('otp-verify');
    setMessage('We sent a login code to your email.');
  };

  const handleVerifyOtp = async () => {
    setMessage('');
    const { error } = await supabase.auth.verifyOtp({ email, token: otpCode, type: 'email' });
    if (error) {
      setMessage(error.message);
      return;
    }
    await afterSignIn();
  };

  return (
    <AuthLayout title="Sign In">
      <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />

      {mode === 'password' && (
        <TextField label="Password" value={password} onChange={(e) => setPassword(e.target.value)} type="password" fullWidth />
      )}

      {mode === 'otp-verify' && (
        <TextField label="Login Code" value={otpCode} onChange={(e) => setOtpCode(e.target.value)} fullWidth />
      )}

      {mode === 'password' && (
        <Button variant="contained" size="large" onClick={handlePasswordLogin}>Login</Button>
      )}
      {mode === 'otp-request' && (
        <Button variant="contained" size="large" onClick={handleSendOtp}>Send Login Code</Button>
      )}
      {mode === 'otp-verify' && (
        <Button variant="contained" size="large" onClick={handleVerifyOtp}>Verify Code</Button>
      )}

      {message ? <Typography color="primary.main">{message}</Typography> : null}

      <Stack spacing={1}>
        {mode === 'password' ? (
          <Link component="button" type="button" onClick={() => { setMode('otp-request'); setMessage(''); }}>
            Use a login code instead
          </Link>
        ) : (
          <Link component="button" type="button" onClick={() => { setMode('password'); setMessage(''); }}>
            Use password instead
          </Link>
        )}
        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
          <Link component={RouterLink} to="/forgot-password">Forgot password?</Link>
          <Link component={RouterLink} to="/register">Create an account</Link>
        </Box>
      </Stack>
    </AuthLayout>
  );
}
