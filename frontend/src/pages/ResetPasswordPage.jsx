import { useEffect, useState } from 'react';
import { Button, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { supabase } from '../services/supabaseClient';
import AuthLayout from '../components/AuthLayout';

export default function ResetPasswordPage() {
  const [ready, setReady] = useState(false);
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('Verifying your reset link...');
  const navigate = useNavigate();

  useEffect(() => {
    const { data: subscription } = supabase.auth.onAuthStateChange((event) => {
      if (event === 'PASSWORD_RECOVERY') {
        setReady(true);
        setMessage('');
      }
    });

    supabase.auth.getSession().then(({ data }) => {
      if (data.session) {
        setReady(true);
        setMessage('');
      }
    });

    return () => subscription.subscription.unsubscribe();
  }, []);

  const handleSubmit = async () => {
    setMessage('');
    const { error } = await supabase.auth.updateUser({ password });
    if (error) {
      setMessage(error.message);
      return;
    }
    navigate('/dashboard');
  };

  return (
    <AuthLayout title="Reset Password">
      {ready ? (
        <>
          <TextField label="New Password" value={password} onChange={(e) => setPassword(e.target.value)} type="password" fullWidth />
          <Button variant="contained" size="large" onClick={handleSubmit}>Update Password</Button>
        </>
      ) : null}
      {message ? <Typography color="primary.main">{message}</Typography> : null}
    </AuthLayout>
  );
}
