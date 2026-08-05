import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { createUser, listBranches, listDepartments, listRoles, listUsers, updateUserStatus } from '../services/api';

const emptyForm = { firstName: '', lastName: '', email: '', roleId: '', departmentId: '', branchId: '' };

export default function UsersListPage() {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [branches, setBranches] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [message, setMessage] = useState('');

  const loadData = async () => {
    const [usersResult, rolesResult, departmentsResult, branchesResult] = await Promise.all([
      listUsers(),
      listRoles(),
      listDepartments(),
      listBranches(),
    ]);
    setUsers(usersResult);
    setRoles(rolesResult);
    setDepartments(departmentsResult);
    setBranches(branchesResult);
  };

  useEffect(() => {
    loadData().catch((error) => setMessage(error.message));
  }, []);

  const handleCreate = async () => {
    setMessage('');
    try {
      await createUser({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        roleId: form.roleId,
        departmentId: form.departmentId || null,
        branchId: form.branchId || null,
      });
      setForm(emptyForm);
      await loadData();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleToggleStatus = async (user) => {
    try {
      await updateUserStatus(user.id, !user.isActive);
      await loadData();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Users</Typography>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Invite User</Typography>
              <Stack direction="row" spacing={2} flexWrap="wrap">
                <TextField label="First Name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
                <TextField label="Last Name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
                <TextField label="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
                <TextField select label="Role" value={form.roleId} onChange={(e) => setForm({ ...form, roleId: e.target.value })} sx={{ minWidth: 160 }}>
                  {roles.map((role) => (
                    <MenuItem key={role.id} value={role.id}>{role.name}</MenuItem>
                  ))}
                </TextField>
                <TextField select label="Department" value={form.departmentId} onChange={(e) => setForm({ ...form, departmentId: e.target.value })} sx={{ minWidth: 160 }}>
                  <MenuItem value="">None</MenuItem>
                  {departments.map((department) => (
                    <MenuItem key={department.id} value={department.id}>{department.name}</MenuItem>
                  ))}
                </TextField>
                <TextField select label="Branch" value={form.branchId} onChange={(e) => setForm({ ...form, branchId: e.target.value })} sx={{ minWidth: 160 }}>
                  <MenuItem value="">None</MenuItem>
                  {branches.map((branch) => (
                    <MenuItem key={branch.id} value={branch.id}>{branch.name}</MenuItem>
                  ))}
                </TextField>
                <Button variant="contained" onClick={handleCreate}>Invite</Button>
              </Stack>
              {message ? <Typography color="error">{message}</Typography> : null}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Role</TableCell>
                    <TableCell>Department</TableCell>
                    <TableCell>Branch</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Action</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell>
                        <RouterLink to={`/users/${user.id}`}>{user.firstName} {user.lastName}</RouterLink>
                      </TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>{user.roleName}</TableCell>
                      <TableCell>{user.departmentName ?? '-'}</TableCell>
                      <TableCell>{user.branchName ?? '-'}</TableCell>
                      <TableCell>
                        <Chip label={user.isActive ? 'Active' : 'Inactive'} color={user.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <Button size="small" onClick={() => handleToggleStatus(user)}>
                          {user.isActive ? 'Deactivate' : 'Activate'}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
