import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
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
import {
  checkInDriver,
  checkOutDriver,
  getMyDriverAssignment,
  getMyDriverAttendance,
  getMyDriverPerformance,
  getMyDriverProfile,
  getMyDriverRatings,
  getMyDriverSalary,
  getMyDriverTrips,
  updateMyDriverProfile,
} from '../services/api';

export default function DriverAppDashboardPage() {
  const [profile, setProfile] = useState(null);
  const [form, setForm] = useState(null);
  const [assignment, setAssignment] = useState(null);
  const [trips, setTrips] = useState([]);
  const [attendance, setAttendance] = useState([]);
  const [salary, setSalary] = useState([]);
  const [ratings, setRatings] = useState([]);
  const [performance, setPerformance] = useState(null);
  const [message, setMessage] = useState('');
  const [notDriver, setNotDriver] = useState(false);

  const loadAll = () => {
    getMyDriverProfile()
      .then((p) => { setProfile(p); setForm(p); })
      .catch((error) => {
        if (error.message?.toLowerCase().includes('not found')) setNotDriver(true);
        else setMessage(error.message);
      });
    getMyDriverAssignment().then(setAssignment).catch(() => {});
    getMyDriverTrips().then(setTrips).catch(() => {});
    getMyDriverAttendance().then(setAttendance).catch(() => {});
    getMyDriverSalary().then(setSalary).catch(() => {});
    getMyDriverRatings().then(setRatings).catch(() => {});
    getMyDriverPerformance().then(setPerformance).catch(() => {});
  };

  useEffect(() => { loadAll(); }, []);

  const today = new Date().toISOString().slice(0, 10);
  const todayRecord = attendance.find((a) => a.attendanceDate?.slice(0, 10) === today);

  const handleCheckIn = async () => {
    setMessage('');
    try {
      await checkInDriver();
      getMyDriverAttendance().then(setAttendance);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleCheckOut = async () => {
    setMessage('');
    try {
      await checkOutDriver();
      getMyDriverAttendance().then(setAttendance);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleSaveProfile = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      const updated = await updateMyDriverProfile({
        photoUrl: form.photoUrl || null,
        address: form.address || null,
        emergencyContactName: form.emergencyContactName || null,
        emergencyContactPhone: form.emergencyContactPhone || null,
      });
      setProfile(updated);
      setForm(updated);
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (notDriver) {
    return (
      <Box sx={{ p: 4 }}>
        <Typography variant="h4" fontWeight={700} sx={{ mb: 2 }}>Driver App</Typography>
        <Typography color="text.secondary">Your account isn't registered as a driver.</Typography>
      </Box>
    );
  }

  if (!profile || !form) {
    return <Box sx={{ p: 4 }}>{message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}</Box>;
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Welcome, {profile.name}</Typography>

        {message ? <Typography color="error">{message}</Typography> : null}

        <Grid container spacing={2}>
          <Grid item xs={12} sm={6} md={3}>
            <Card><CardContent>
              <Typography color="text.secondary">Current Vehicle</Typography>
              <Typography variant="h6">{assignment?.vehicleRegistrationNumber ?? 'Unassigned'}</Typography>
            </CardContent></Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card><CardContent>
              <Typography color="text.secondary">Rating</Typography>
              <Typography variant="h6">{profile.rating ?? '-'}</Typography>
            </CardContent></Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card><CardContent>
              <Typography color="text.secondary">Trips This Month</Typography>
              <Typography variant="h6">{performance?.tripsThisMonth ?? '-'}</Typography>
            </CardContent></Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card><CardContent>
              <Typography color="text.secondary">Attendance Rate</Typography>
              <Typography variant="h6">{performance ? `${performance.attendanceRateThisMonth}%` : '-'}</Typography>
            </CardContent></Card>
          </Grid>
        </Grid>

        <Card>
          <CardContent>
            <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap">
              <Box>
                <Typography variant="h6">Today's Attendance</Typography>
                <Typography color="text.secondary">
                  {todayRecord?.checkInAt ? `Checked in at ${new Date(todayRecord.checkInAt).toLocaleTimeString()}` : 'Not checked in yet'}
                  {todayRecord?.checkOutAt ? ` · Checked out at ${new Date(todayRecord.checkOutAt).toLocaleTimeString()}` : ''}
                </Typography>
              </Box>
              <Stack direction="row" spacing={2}>
                <Button variant="contained" onClick={handleCheckIn} disabled={Boolean(todayRecord?.checkInAt)}>Check In</Button>
                <Button variant="outlined" onClick={handleCheckOut} disabled={!todayRecord?.checkInAt || Boolean(todayRecord?.checkOutAt)}>Check Out</Button>
              </Stack>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>My Profile</Typography>
            <Stack component="form" onSubmit={handleSaveProfile} direction="row" spacing={2} flexWrap="wrap" alignItems="center">
              <TextField label="Address" value={form.address ?? ''} onChange={(e) => setForm({ ...form, address: e.target.value })} sx={{ minWidth: 240 }} />
              <TextField label="Emergency Contact Name" value={form.emergencyContactName ?? ''} onChange={(e) => setForm({ ...form, emergencyContactName: e.target.value })} sx={{ minWidth: 200 }} />
              <TextField label="Emergency Contact Phone" value={form.emergencyContactPhone ?? ''} onChange={(e) => setForm({ ...form, emergencyContactPhone: e.target.value })} sx={{ minWidth: 200 }} />
              <Button type="submit" variant="contained">Save</Button>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Recent Trips</Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Started</TableCell>
                    <TableCell>Ended</TableCell>
                    <TableCell>Distance (km)</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {trips.slice(0, 10).map((trip) => (
                    <TableRow key={trip.id}>
                      <TableCell>{new Date(trip.startedAt).toLocaleString()}</TableCell>
                      <TableCell>{trip.endedAt ? new Date(trip.endedAt).toLocaleString() : '-'}</TableCell>
                      <TableCell>{trip.distanceKm}</TableCell>
                      <TableCell><Chip size="small" label={trip.status} /></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Payslips</Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Period</TableCell>
                    <TableCell>Net Amount</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {salary.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell>{new Date(s.periodStart).toLocaleDateString()} - {new Date(s.periodEnd).toLocaleDateString()}</TableCell>
                      <TableCell>{s.netAmount}</TableCell>
                      <TableCell><Chip size="small" label={s.status} color={s.status === 'Paid' ? 'success' : 'default'} /></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Ratings Received</Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Score</TableCell>
                    <TableCell>Comment</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {ratings.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell>{new Date(r.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell>{r.category}</TableCell>
                      <TableCell>{r.score}</TableCell>
                      <TableCell>{r.comment ?? '-'}</TableCell>
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
