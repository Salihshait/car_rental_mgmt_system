import { useState } from 'react';
import { Button, Link, TextField, Typography } from '@mui/material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { supabase } from '../services/supabaseClient';
import { completeProfile } from '../services/api';
import AuthLayout from '../components/AuthLayout';

export default function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleRegister = async () => {
    setMessage('');
    const { data, error } = await supabase.auth.signUp({ email, password });
    if (error) {
      setMessage(error.message);
      return;
    }

    if (data.session) {
      await completeProfile({ firstName, lastName });
      navigate('/dashboard');
      return;
    }

    setMessage('Account created. Check your email to confirm it, then log in.');
  };

  return (
    <AuthLayout title="Create Account">
      <TextField label="First Name" value={firstName} onChange={(e) => setFirstName(e.target.value)} fullWidth />
      <TextField label="Last Name" value={lastName} onChange={(e) => setLastName(e.target.value)} fullWidth />
      <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />
      <TextField label="Password" value={password} onChange={(e) => setPassword(e.target.value)} type="password" fullWidth />
      <Button variant="contained" size="large" onClick={handleRegister}>Register</Button>
      {message ? <Typography color="primary.main">{message}</Typography> : null}
      <Link component={RouterLink} to="/login">Already have an account? Sign in</Link>
    </AuthLayout>
  );
}
