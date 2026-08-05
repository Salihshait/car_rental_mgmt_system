import { useState } from 'react';
import { Button, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { completeProfile } from '../services/api';
import AuthLayout from '../components/AuthLayout';

export default function CompleteProfilePage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async () => {
    setMessage('');
    try {
      await completeProfile({ firstName, lastName, phoneNumber });
      navigate('/dashboard');
    } catch (error) {
      setMessage(error.message ?? 'Unable to complete profile.');
    }
  };

  return (
    <AuthLayout title="Complete Your Profile">
      <TextField label="First Name" value={firstName} onChange={(e) => setFirstName(e.target.value)} fullWidth />
      <TextField label="Last Name" value={lastName} onChange={(e) => setLastName(e.target.value)} fullWidth />
      <TextField label="Phone Number" value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} fullWidth />
      <Button variant="contained" size="large" onClick={handleSubmit}>Continue</Button>
      {message ? <Typography color="error">{message}</Typography> : null}
    </AuthLayout>
  );
}
