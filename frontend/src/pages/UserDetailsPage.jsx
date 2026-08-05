import { useEffect, useState } from 'react';
import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import {
  getUser,
  listBranches,
  listDepartments,
  listRoles,
  logoutAllForUser,
  updateUser,
  updateUserStatus,
  uploadUserAvatar,
} from '../services/api';

export default function UserDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [roles, setRoles] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [branches, setBranches] = useState([]);
  const [message, setMessage] = useState('');

  const load = async () => {
    const [userResult, rolesResult, departmentsResult, branchesResult] = await Promise.all([
      getUser(id),
      listRoles(),
      listDepartments(),
      listBranches(),
    ]);
    setUser(userResult);
    setRoles(rolesResult);
    setDepartments(departmentsResult);
    setBranches(branchesResult);
  };

  useEffect(() => {
    load().catch((error) => setMessage(error.message));
  }, [id]);

  const handleSave = async () => {
    setMessage('');
    try {
      const updated = await updateUser(id, {
        firstName: user.firstName,
        lastName: user.lastName,
        phoneNumber: user.phoneNumber,
        roleId: user.roleId,
        departmentId: user.departmentId || null,
        branchId: user.branchId || null,
      });
      setUser(updated);
      setMessage('Saved.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleToggleStatus = async () => {
    try {
      const updated = await updateUserStatus(id, !user.isActive);
      setUser(updated);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAvatarChange = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;
    try {
      const updated = await uploadUserAvatar(id, file);
      setUser(updated);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleLogoutAll = async () => {
    try {
      await logoutAllForUser(id);
      setMessage('All sessions for this user have been revoked.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (!user) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 640 }}>
        <Button onClick={() => navigate('/users')} sx={{ alignSelf: 'flex-start' }}>Back to Users</Button>
        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" spacing={2} alignItems="center">
                <Avatar src={user.avatarUrl ?? undefined} sx={{ width: 64, height: 64 }} />
                <Button component="label" size="small">
                  Change Avatar
                  <input type="file" accept="image/*" hidden onChange={handleAvatarChange} />
                </Button>
                <Chip label={user.isActive ? 'Active' : 'Inactive'} color={user.isActive ? 'success' : 'default'} size="small" />
              </Stack>

              <TextField label="First Name" value={user.firstName} onChange={(e) => setUser({ ...user, firstName: e.target.value })} />
              <TextField label="Last Name" value={user.lastName} onChange={(e) => setUser({ ...user, lastName: e.target.value })} />
              <TextField label="Email" value={user.email} disabled />
              <TextField label="Phone Number" value={user.phoneNumber ?? ''} onChange={(e) => setUser({ ...user, phoneNumber: e.target.value })} />

              <TextField select label="Role" value={user.roleId} onChange={(e) => setUser({ ...user, roleId: e.target.value })}>
                {roles.map((role) => (
                  <MenuItem key={role.id} value={role.id}>{role.name}</MenuItem>
                ))}
              </TextField>

              <TextField select label="Department" value={user.departmentId ?? ''} onChange={(e) => setUser({ ...user, departmentId: e.target.value })}>
                <MenuItem value="">None</MenuItem>
                {departments.map((department) => (
                  <MenuItem key={department.id} value={department.id}>{department.name}</MenuItem>
                ))}
              </TextField>

              <TextField select label="Branch" value={user.branchId ?? ''} onChange={(e) => setUser({ ...user, branchId: e.target.value })}>
                <MenuItem value="">None</MenuItem>
                {branches.map((branch) => (
                  <MenuItem key={branch.id} value={branch.id}>{branch.name}</MenuItem>
                ))}
              </TextField>

              <Stack direction="row" spacing={2}>
                <Button variant="contained" onClick={handleSave}>Save</Button>
                <Button variant="outlined" onClick={handleToggleStatus}>
                  {user.isActive ? 'Deactivate' : 'Activate'}
                </Button>
                <Button variant="outlined" color="warning" onClick={handleLogoutAll}>
                  Logout All Devices
                </Button>
              </Stack>

              {message ? <Typography color="primary.main">{message}</Typography> : null}
            </Stack>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
