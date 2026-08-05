import { useEffect, useState } from 'react';
import { Avatar, Box, Button, Card, CardContent, Chip, Stack, TextField, Typography } from '@mui/material';
import { supabase } from '../services/supabaseClient';
import { getMyProfile, logoutAllMyDevices, updateMyProfile, uploadMyAvatar } from '../services/api';

export default function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [newPassword, setNewPassword] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    getMyProfile().then(setProfile).catch((error) => setMessage(error.message));
  }, []);

  const handleSave = async () => {
    setMessage('');
    try {
      const updated = await updateMyProfile({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneNumber: profile.phoneNumber,
      });
      setProfile(updated);
      setMessage('Profile updated.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleChangePassword = async () => {
    setMessage('');
    const { error } = await supabase.auth.updateUser({ password: newPassword });
    if (error) {
      setMessage(error.message);
      return;
    }
    setNewPassword('');
    setMessage('Password changed.');
  };

  const handleAvatarChange = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;
    try {
      const updated = await uploadMyAvatar(file);
      setProfile(updated);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleLogoutAll = async () => {
    try {
      await logoutAllMyDevices();
      setMessage('Logged out of all devices. You may need to sign in again here too.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (!profile) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 480 }}>
        <Typography variant="h4" fontWeight={700}>My Profile</Typography>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" spacing={2} alignItems="center">
                <Avatar src={profile.avatarUrl ?? undefined} sx={{ width: 64, height: 64 }} />
                <Button component="label" size="small">
                  Change Avatar
                  <input type="file" accept="image/*" hidden onChange={handleAvatarChange} />
                </Button>
                <Chip label={profile.roleName} size="small" />
              </Stack>

              <TextField label="First Name" value={profile.firstName} onChange={(e) => setProfile({ ...profile, firstName: e.target.value })} />
              <TextField label="Last Name" value={profile.lastName} onChange={(e) => setProfile({ ...profile, lastName: e.target.value })} />
              <TextField label="Email" value={profile.email} disabled />
              <TextField label="Phone Number" value={profile.phoneNumber ?? ''} onChange={(e) => setProfile({ ...profile, phoneNumber: e.target.value })} />
              <Button variant="contained" onClick={handleSave}>Save Profile</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Change Password</Typography>
              <TextField label="New Password" type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
              <Button variant="outlined" onClick={handleChangePassword}>Update Password</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Sessions</Typography>
              <Button variant="outlined" color="warning" onClick={handleLogoutAll}>Logout All Devices</Button>
            </Stack>
          </CardContent>
        </Card>

        {message ? <Typography color="primary.main">{message}</Typography> : null}
      </Stack>
    </Box>
  );
}
