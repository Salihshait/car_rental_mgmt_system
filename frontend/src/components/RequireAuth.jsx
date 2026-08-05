import { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { supabase } from '../services/supabaseClient';

export default function RequireAuth({ children }) {
  const [status, setStatus] = useState('loading');

  useEffect(() => {
    supabase.auth.getSession().then(({ data }) => {
      setStatus(data.session ? 'authenticated' : 'anonymous');
    });

    const { data: subscription } = supabase.auth.onAuthStateChange((_event, session) => {
      setStatus(session ? 'authenticated' : 'anonymous');
    });

    return () => subscription.subscription.unsubscribe();
  }, []);

  if (status === 'loading') {
    return (
      <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (status === 'anonymous') {
    return <Navigate to="/login" replace />;
  }

  return children;
}
