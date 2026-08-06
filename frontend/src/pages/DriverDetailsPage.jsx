import { useEffect, useState } from 'react';
import { useParams, Link as RouterLink } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
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
import {
  addDriverRating,
  generateDriverSalary,
  getDriver,
  getDriverAttendance,
  getDriverPerformance,
  listBranches,
  listDepartments,
  listDriverDocuments,
  listDriverRatings,
  listDriverSalary,
  markDriverAttendance,
  markDriverSalaryPaid,
  updateDriver,
  uploadDriverDocument,
  verifyDriverDocument,
} from '../services/api';

const EMPLOYMENT_STATUSES = ['Active', 'OnLeave', 'Terminated'];
const KYC_STATUSES = ['Pending', 'Verified', 'Rejected'];

export default function DriverDetailsPage() {
  const { id } = useParams();
  const [driver, setDriver] = useState(null);
  const [form, setForm] = useState(null);
  const [departments, setDepartments] = useState([]);
  const [branches, setBranches] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [docForm, setDocForm] = useState({ documentType: 'DrivingLicense', documentNumber: '', expiryDate: '', file: null });
  const [attendance, setAttendance] = useState([]);
  const [attendanceForm, setAttendanceForm] = useState({ attendanceDate: '', status: 'Absent', notes: '' });
  const [salary, setSalary] = useState([]);
  const [salaryForm, setSalaryForm] = useState({ periodStart: '', periodEnd: '', baseAmount: '', deductions: '0', bonus: '0', notes: '' });
  const [ratings, setRatings] = useState([]);
  const [ratingForm, setRatingForm] = useState({ score: 5, category: 'Overall', comment: '' });
  const [performance, setPerformance] = useState(null);
  const [message, setMessage] = useState('');

  const loadAll = () => {
    getDriver(id).then((d) => { setDriver(d); setForm(d); }).catch((error) => setMessage(error.message));
    listDriverDocuments(id).then(setDocuments).catch((error) => setMessage(error.message));
    getDriverAttendance(id).then(setAttendance).catch((error) => setMessage(error.message));
    listDriverSalary(id).then(setSalary).catch((error) => setMessage(error.message));
    listDriverRatings(id).then(setRatings).catch((error) => setMessage(error.message));
    getDriverPerformance(id).then(setPerformance).catch((error) => setMessage(error.message));
  };

  useEffect(() => {
    listDepartments().then(setDepartments).catch((error) => setMessage(error.message));
    listBranches().then(setBranches).catch((error) => setMessage(error.message));
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleSaveProfile = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await updateDriver(id, {
        licenseNumber: form.licenseNumber,
        kycStatus: form.kycStatus,
        photoUrl: form.photoUrl || null,
        address: form.address || null,
        emergencyContactName: form.emergencyContactName || null,
        emergencyContactPhone: form.emergencyContactPhone || null,
        dateOfJoining: form.dateOfJoining || null,
        employmentStatus: form.employmentStatus,
        departmentId: form.departmentId || null,
        branchId: form.branchId || null,
        licenseExpiryDate: form.licenseExpiryDate || null,
        baseSalary: form.baseSalary === '' || form.baseSalary == null ? null : Number(form.baseSalary),
      });
      loadAll();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleUploadDocument = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await uploadDriverDocument(id, docForm);
      setDocForm({ documentType: 'DrivingLicense', documentNumber: '', expiryDate: '', file: null });
      listDriverDocuments(id).then(setDocuments);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleVerifyDocument = async (documentId, status) => {
    try {
      await verifyDriverDocument(id, documentId, status);
      listDriverDocuments(id).then(setDocuments);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleMarkAttendance = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await markDriverAttendance(id, attendanceForm);
      setAttendanceForm({ attendanceDate: '', status: 'Absent', notes: '' });
      getDriverAttendance(id).then(setAttendance);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleGenerateSalary = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await generateDriverSalary({
        driverId: id,
        periodStart: salaryForm.periodStart,
        periodEnd: salaryForm.periodEnd,
        baseAmount: Number(salaryForm.baseAmount),
        deductions: Number(salaryForm.deductions || 0),
        bonus: Number(salaryForm.bonus || 0),
        notes: salaryForm.notes || null,
      });
      setSalaryForm({ periodStart: '', periodEnd: '', baseAmount: '', deductions: '0', bonus: '0', notes: '' });
      listDriverSalary(id).then(setSalary);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleMarkPaid = async (paymentId) => {
    try {
      await markDriverSalaryPaid(paymentId);
      listDriverSalary(id).then(setSalary);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAddRating = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      await addDriverRating({ driverId: id, score: Number(ratingForm.score), category: ratingForm.category, comment: ratingForm.comment || null });
      setRatingForm({ score: 5, category: 'Overall', comment: '' });
      listDriverRatings(id).then(setRatings);
      getDriver(id).then((d) => { setDriver(d); setForm(d); });
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (!driver || !form) {
    return <Box sx={{ p: 4 }}>{message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}</Box>;
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>{driver.name}</Typography>
          <Button component={RouterLink} to="/fleet/drivers" variant="outlined">Manage Vehicle Assignment</Button>
        </Stack>

        {message ? <Typography color="error">{message}</Typography> : null}

        {performance ? (
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Trips This Month</Typography><Typography variant="h5">{performance.tripsThisMonth}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Distance This Month (km)</Typography><Typography variant="h5">{performance.distanceThisMonthKm}</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Attendance Rate</Typography><Typography variant="h5">{performance.attendanceRateThisMonth}%</Typography></CardContent></Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card><CardContent><Typography color="text.secondary">Average Rating</Typography><Typography variant="h5">{performance.averageRating ?? '-'} ({performance.ratingCount})</Typography></CardContent></Card>
            </Grid>
          </Grid>
        ) : null}

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Profile</Typography>
            <Stack component="form" onSubmit={handleSaveProfile} spacing={2}>
              <Stack direction="row" spacing={2} flexWrap="wrap">
                <TextField label="License Number" required value={form.licenseNumber} onChange={(e) => setForm({ ...form, licenseNumber: e.target.value })} sx={{ minWidth: 200 }} />
                <TextField select label="KYC Status" value={form.kycStatus} onChange={(e) => setForm({ ...form, kycStatus: e.target.value })} sx={{ minWidth: 160 }}>
                  {KYC_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </TextField>
                <TextField select label="Employment Status" value={form.employmentStatus} onChange={(e) => setForm({ ...form, employmentStatus: e.target.value })} sx={{ minWidth: 160 }}>
                  {EMPLOYMENT_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </TextField>
                <TextField
                  label="License Expiry"
                  type="date"
                  InputLabelProps={{ shrink: true }}
                  value={form.licenseExpiryDate ? form.licenseExpiryDate.slice(0, 10) : ''}
                  onChange={(e) => setForm({ ...form, licenseExpiryDate: e.target.value })}
                  sx={{ minWidth: 180 }}
                />
              </Stack>
              <Stack direction="row" spacing={2} flexWrap="wrap">
                <TextField select label="Department" value={form.departmentId ?? ''} onChange={(e) => setForm({ ...form, departmentId: e.target.value })} sx={{ minWidth: 180 }}>
                  <MenuItem value="">None</MenuItem>
                  {departments.map((d) => <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>)}
                </TextField>
                <TextField select label="Branch" value={form.branchId ?? ''} onChange={(e) => setForm({ ...form, branchId: e.target.value })} sx={{ minWidth: 180 }}>
                  <MenuItem value="">None</MenuItem>
                  {branches.map((b) => <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>)}
                </TextField>
                <TextField
                  label="Date of Joining"
                  type="date"
                  InputLabelProps={{ shrink: true }}
                  value={form.dateOfJoining ? form.dateOfJoining.slice(0, 10) : ''}
                  onChange={(e) => setForm({ ...form, dateOfJoining: e.target.value })}
                  sx={{ minWidth: 180 }}
                />
                <TextField label="Base Salary" type="number" value={form.baseSalary ?? ''} onChange={(e) => setForm({ ...form, baseSalary: e.target.value })} sx={{ minWidth: 160 }} />
              </Stack>
              <Stack direction="row" spacing={2} flexWrap="wrap">
                <TextField label="Address" value={form.address ?? ''} onChange={(e) => setForm({ ...form, address: e.target.value })} sx={{ minWidth: 260 }} />
                <TextField label="Emergency Contact Name" value={form.emergencyContactName ?? ''} onChange={(e) => setForm({ ...form, emergencyContactName: e.target.value })} sx={{ minWidth: 200 }} />
                <TextField label="Emergency Contact Phone" value={form.emergencyContactPhone ?? ''} onChange={(e) => setForm({ ...form, emergencyContactPhone: e.target.value })} sx={{ minWidth: 200 }} />
              </Stack>
              <Box><Button type="submit" variant="contained">Save Profile</Button></Box>
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>License & Documents</Typography>
            <Stack component="form" onSubmit={handleUploadDocument} direction="row" spacing={2} flexWrap="wrap" alignItems="center" sx={{ mb: 2 }}>
              <TextField label="Document Type" required value={docForm.documentType} onChange={(e) => setDocForm({ ...docForm, documentType: e.target.value })} sx={{ minWidth: 180 }} />
              <TextField label="Document Number" value={docForm.documentNumber} onChange={(e) => setDocForm({ ...docForm, documentNumber: e.target.value })} sx={{ minWidth: 180 }} />
              <TextField label="Expiry Date" type="date" InputLabelProps={{ shrink: true }} value={docForm.expiryDate} onChange={(e) => setDocForm({ ...docForm, expiryDate: e.target.value })} sx={{ minWidth: 180 }} />
              <Button component="label" variant="outlined">
                {docForm.file ? docForm.file.name : 'Choose File'}
                <input type="file" hidden onChange={(e) => setDocForm({ ...docForm, file: e.target.files?.[0] ?? null })} />
              </Button>
              <Button type="submit" variant="contained">Upload</Button>
            </Stack>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Type</TableCell>
                    <TableCell>Number</TableCell>
                    <TableCell>Expiry</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {documents.map((doc) => (
                    <TableRow key={doc.id}>
                      <TableCell>{doc.documentType}</TableCell>
                      <TableCell>{doc.documentNumber ?? '-'}</TableCell>
                      <TableCell>{doc.expiryDate ? new Date(doc.expiryDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell><Chip size="small" label={doc.verificationStatus} /></TableCell>
                      <TableCell>
                        {doc.verificationStatus === 'Pending' ? (
                          <Stack direction="row" spacing={1}>
                            <Button size="small" onClick={() => handleVerifyDocument(doc.id, 'Verified')}>Verify</Button>
                            <Button size="small" color="error" onClick={() => handleVerifyDocument(doc.id, 'Rejected')}>Reject</Button>
                          </Stack>
                        ) : null}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Attendance</Typography>
            <Stack component="form" onSubmit={handleMarkAttendance} direction="row" spacing={2} flexWrap="wrap" alignItems="center" sx={{ mb: 2 }}>
              <TextField label="Date" type="date" required InputLabelProps={{ shrink: true }} value={attendanceForm.attendanceDate} onChange={(e) => setAttendanceForm({ ...attendanceForm, attendanceDate: e.target.value })} sx={{ minWidth: 180 }} />
              <TextField select label="Status" value={attendanceForm.status} onChange={(e) => setAttendanceForm({ ...attendanceForm, status: e.target.value })} sx={{ minWidth: 160 }}>
                <MenuItem value="Present">Present</MenuItem>
                <MenuItem value="Absent">Absent</MenuItem>
                <MenuItem value="Leave">Leave</MenuItem>
                <MenuItem value="HalfDay">Half Day</MenuItem>
              </TextField>
              <TextField label="Notes" value={attendanceForm.notes} onChange={(e) => setAttendanceForm({ ...attendanceForm, notes: e.target.value })} sx={{ minWidth: 200 }} />
              <Button type="submit" variant="contained">Mark</Button>
            </Stack>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Check In</TableCell>
                    <TableCell>Check Out</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Notes</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {attendance.map((a) => (
                    <TableRow key={a.id}>
                      <TableCell>{new Date(a.attendanceDate).toLocaleDateString()}</TableCell>
                      <TableCell>{a.checkInAt ? new Date(a.checkInAt).toLocaleTimeString() : '-'}</TableCell>
                      <TableCell>{a.checkOutAt ? new Date(a.checkOutAt).toLocaleTimeString() : '-'}</TableCell>
                      <TableCell>{a.status}</TableCell>
                      <TableCell>{a.notes ?? '-'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Salary</Typography>
            <Stack component="form" onSubmit={handleGenerateSalary} direction="row" spacing={2} flexWrap="wrap" alignItems="center" sx={{ mb: 2 }}>
              <TextField label="Period Start" type="date" required InputLabelProps={{ shrink: true }} value={salaryForm.periodStart} onChange={(e) => setSalaryForm({ ...salaryForm, periodStart: e.target.value })} sx={{ minWidth: 170 }} />
              <TextField label="Period End" type="date" required InputLabelProps={{ shrink: true }} value={salaryForm.periodEnd} onChange={(e) => setSalaryForm({ ...salaryForm, periodEnd: e.target.value })} sx={{ minWidth: 170 }} />
              <TextField label="Base Amount" type="number" required value={salaryForm.baseAmount} onChange={(e) => setSalaryForm({ ...salaryForm, baseAmount: e.target.value })} sx={{ width: 140 }} />
              <TextField label="Deductions" type="number" value={salaryForm.deductions} onChange={(e) => setSalaryForm({ ...salaryForm, deductions: e.target.value })} sx={{ width: 140 }} />
              <TextField label="Bonus" type="number" value={salaryForm.bonus} onChange={(e) => setSalaryForm({ ...salaryForm, bonus: e.target.value })} sx={{ width: 140 }} />
              <Button type="submit" variant="contained">Generate Payslip</Button>
            </Stack>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Period</TableCell>
                    <TableCell>Base</TableCell>
                    <TableCell>Deductions</TableCell>
                    <TableCell>Bonus</TableCell>
                    <TableCell>Net</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {salary.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell>{new Date(s.periodStart).toLocaleDateString()} - {new Date(s.periodEnd).toLocaleDateString()}</TableCell>
                      <TableCell>{s.baseAmount}</TableCell>
                      <TableCell>{s.deductions}</TableCell>
                      <TableCell>{s.bonus}</TableCell>
                      <TableCell>{s.netAmount}</TableCell>
                      <TableCell><Chip size="small" label={s.status} color={s.status === 'Paid' ? 'success' : 'default'} /></TableCell>
                      <TableCell>
                        {s.status === 'Pending' ? <Button size="small" onClick={() => handleMarkPaid(s.id)}>Mark Paid</Button> : null}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Performance Ratings</Typography>
            <Stack component="form" onSubmit={handleAddRating} direction="row" spacing={2} flexWrap="wrap" alignItems="center" sx={{ mb: 2 }}>
              <TextField select label="Score" value={ratingForm.score} onChange={(e) => setRatingForm({ ...ratingForm, score: e.target.value })} sx={{ width: 100 }}>
                {[1, 2, 3, 4, 5].map((n) => <MenuItem key={n} value={n}>{n}</MenuItem>)}
              </TextField>
              <TextField label="Category" value={ratingForm.category} onChange={(e) => setRatingForm({ ...ratingForm, category: e.target.value })} sx={{ minWidth: 160 }} />
              <TextField label="Comment" value={ratingForm.comment} onChange={(e) => setRatingForm({ ...ratingForm, comment: e.target.value })} sx={{ minWidth: 240 }} />
              <Button type="submit" variant="contained">Add Rating</Button>
            </Stack>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Rated By</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Score</TableCell>
                    <TableCell>Comment</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {ratings.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell>{new Date(r.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell>{r.ratedByName ?? '-'}</TableCell>
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
