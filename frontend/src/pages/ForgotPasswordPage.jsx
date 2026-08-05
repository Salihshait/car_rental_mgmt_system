import { useState } from 'react';
import { Button, Link, TextField, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { supabase } from '../services/supabaseClient';
import AuthLayout from '../components/AuthLayout';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async () => {
    setMessage('');
    const { error } = await supabase.auth.resetPasswordForEmail(email, {
      redirectTo: `${window.location.origin}/reset-password`,
    });
    if (error) {
      setMessage(error.message);
      return;
    }
    setMessage('If that email has an account, a reset link has been sent.');
  };

  return (
    <AuthLayout title="Forgot Password">
      <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />
      <Button variant="contained" size="large" onClick={handleSubmit}>Send Reset Link</Button>
      {message ? <Typography color="primary.main">{message}</Typography> : null}
      <Link component={RouterLink} to="/login">Back to sign in</Link>
    </AuthLayout>
  );
}
