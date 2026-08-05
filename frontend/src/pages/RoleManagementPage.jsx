import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
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
import { createRole, deleteRole, listRoles } from '../services/api';

const emptyForm = { name: '', description: '' };

export default function RoleManagementPage() {
  const [roles, setRoles] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [message, setMessage] = useState('');

  const load = () => listRoles().then(setRoles).catch((error) => setMessage(error.message));

  useEffect(() => {
    load();
  }, []);

  const handleCreate = async () => {
    setMessage('');
    try {
      await createRole(form);
      setForm(emptyForm);
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDelete = async (id) => {
    setMessage('');
    try {
      await deleteRole(id);
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Role Management</Typography>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">New Role</Typography>
              <Stack direction="row" spacing={2}>
                <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                <TextField label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} sx={{ flexGrow: 1 }} />
                <Button variant="contained" onClick={handleCreate}>Add Role</Button>
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
                    <TableCell>Description</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell align="right">Action</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {roles.map((role) => (
                    <TableRow key={role.id}>
                      <TableCell>{role.name}</TableCell>
                      <TableCell>{role.description ?? '-'}</TableCell>
                      <TableCell>
                        <Chip label={role.isSystem ? 'System' : 'Custom'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        {!role.isSystem && (
                          <Button size="small" color="error" onClick={() => handleDelete(role.id)}>Delete</Button>
                        )}
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
