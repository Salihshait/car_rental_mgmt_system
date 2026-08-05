import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { getPermissionMatrix, updateRolePermissions } from '../services/api';

export default function PermissionMatrixPage() {
  const [matrix, setMatrix] = useState(null);
  const [assignments, setAssignments] = useState({});
  const [message, setMessage] = useState('');
  const [saving, setSaving] = useState(false);

  const load = async () => {
    const result = await getPermissionMatrix();
    setMatrix(result);
    setAssignments(result.rolePermissionIds);
  };

  useEffect(() => {
    load().catch((error) => setMessage(error.message));
  }, []);

  const isChecked = (roleId, permissionId) => (assignments[roleId] ?? []).includes(permissionId);

  const toggle = (roleId, permissionId) => {
    setAssignments((prev) => {
      const current = prev[roleId] ?? [];
      const next = current.includes(permissionId)
        ? current.filter((id) => id !== permissionId)
        : [...current, permissionId];
      return { ...prev, [roleId]: next };
    });
  };

  const handleSave = async () => {
    setSaving(true);
    setMessage('');
    try {
      await Promise.all(
        matrix.roles.map((role) => updateRolePermissions(role.id, assignments[role.id] ?? [])),
      );
      setMessage('Permission matrix saved.');
    } catch (error) {
      setMessage(error.message);
    } finally {
      setSaving(false);
    }
  };

  if (!matrix) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Permission Matrix</Typography>
          <Button variant="contained" onClick={handleSave} disabled={saving}>Save Changes</Button>
        </Stack>

        {message ? <Typography color="primary.main">{message}</Typography> : null}

        <Card>
          <CardContent>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Permission</TableCell>
                    {matrix.roles.map((role) => (
                      <TableCell key={role.id} align="center">{role.name}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {matrix.permissions.map((permission) => (
                    <TableRow key={permission.id}>
                      <TableCell>{permission.name}</TableCell>
                      {matrix.roles.map((role) => (
                        <TableCell key={role.id} align="center">
                          <Checkbox
                            checked={isChecked(role.id, permission.id)}
                            onChange={() => toggle(role.id, permission.id)}
                          />
                        </TableCell>
                      ))}
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
