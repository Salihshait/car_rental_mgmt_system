import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Grid,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import {
  adjustCustomerLoyalty,
  adjustCustomerWallet,
  deleteCustomerDocument,
  getCustomer,
  getCustomerBookings,
  getCustomerTimeline,
  listCustomerDocuments,
  updateCustomer,
  uploadCustomerDocument,
  verifyCustomerDocument,
} from '../services/api';

const KYC_STATUSES = ['Pending', 'Verified', 'Rejected'];
const DOCUMENT_TYPES = ['DriverLicense', 'Passport', 'Aadhaar', 'Other'];

export default function CustomerDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [customer, setCustomer] = useState(null);
  const [bookings, setBookings] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [timeline, setTimeline] = useState([]);
  const [message, setMessage] = useState('');

  const [walletDialog, setWalletDialog] = useState(false);
  const [loyaltyDialog, setLoyaltyDialog] = useState(false);
  const [walletForm, setWalletForm] = useState({ amount: '', reason: '' });
  const [loyaltyForm, setLoyaltyForm] = useState({ points: '', reason: '' });

  const [docDialogOpen, setDocDialogOpen] = useState(false);
  const [docForm, setDocForm] = useState({ documentType: 'DriverLicense', documentNumber: '', expiryDate: '', file: null });

  const load = async () => {
    const [customerResult, bookingsResult, documentsResult, timelineResult] = await Promise.all([
      getCustomer(id), getCustomerBookings(id), listCustomerDocuments(id), getCustomerTimeline(id),
    ]);
    setCustomer(customerResult);
    setBookings(bookingsResult);
    setDocuments(documentsResult);
    setTimeline(timelineResult);
  };

  useEffect(() => {
    load().catch((error) => setMessage(error.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleSave = async () => {
    setMessage('');
    try {
      const updated = await updateCustomer(id, {
        kycStatus: customer.kycStatus,
        isBlacklisted: customer.isBlacklisted,
        isCorporate: customer.isCorporate,
        companyName: customer.companyName,
        emergencyContactName: customer.emergencyContactName,
        emergencyContactPhone: customer.emergencyContactPhone,
        emergencyContactRelation: customer.emergencyContactRelation,
      });
      setCustomer(updated);
      const timelineResult = await getCustomerTimeline(id);
      setTimeline(timelineResult);
      setMessage('Saved.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAdjustWallet = async () => {
    try {
      const updated = await adjustCustomerWallet(id, { amount: Number(walletForm.amount), reason: walletForm.reason });
      setCustomer(updated);
      setWalletDialog(false);
      setWalletForm({ amount: '', reason: '' });
      setTimeline(await getCustomerTimeline(id));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAdjustLoyalty = async () => {
    try {
      const updated = await adjustCustomerLoyalty(id, { points: Number(loyaltyForm.points), reason: loyaltyForm.reason });
      setCustomer(updated);
      setLoyaltyDialog(false);
      setLoyaltyForm({ points: '', reason: '' });
      setTimeline(await getCustomerTimeline(id));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleUploadDocument = async () => {
    try {
      await uploadCustomerDocument(id, docForm);
      setDocDialogOpen(false);
      setDocForm({ documentType: 'DriverLicense', documentNumber: '', expiryDate: '', file: null });
      setDocuments(await listCustomerDocuments(id));
      setTimeline(await getCustomerTimeline(id));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleVerifyDocument = async (docId, status) => {
    try {
      await verifyCustomerDocument(id, docId, status);
      setDocuments(await listCustomerDocuments(id));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDeleteDocument = async (docId) => {
    try {
      await deleteCustomerDocument(id, docId);
      setDocuments((prev) => prev.filter((d) => d.id !== docId));
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (!customer) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 900 }}>
        <Button onClick={() => navigate('/customers')} sx={{ alignSelf: 'flex-start' }}>Back to Customers</Button>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h5">{customer.firstName} {customer.lastName}</Typography>
              <Typography color="text.secondary">{customer.email}</Typography>

              <Grid container spacing={2}>
                <Grid item xs={12} sm={4}>
                  <TextField select fullWidth label="KYC Status" value={customer.kycStatus} onChange={(e) => setCustomer({ ...customer, kycStatus: e.target.value })}>
                    {KYC_STATUSES.map((status) => <MenuItem key={status} value={status}>{status}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <FormControlLabel
                    control={<Checkbox checked={customer.isBlacklisted} onChange={(e) => setCustomer({ ...customer, isBlacklisted: e.target.checked })} />}
                    label="Blacklisted"
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <FormControlLabel
                    control={<Checkbox checked={customer.isCorporate} onChange={(e) => setCustomer({ ...customer, isCorporate: e.target.checked })} />}
                    label="Corporate Customer"
                  />
                </Grid>
              </Grid>

              {customer.isCorporate && (
                <TextField label="Company Name" value={customer.companyName ?? ''} onChange={(e) => setCustomer({ ...customer, companyName: e.target.value })} />
              )}

              <Typography variant="subtitle2">Emergency Contact</Typography>
              <Stack direction="row" spacing={2}>
                <TextField label="Name" value={customer.emergencyContactName ?? ''} onChange={(e) => setCustomer({ ...customer, emergencyContactName: e.target.value })} fullWidth />
                <TextField label="Phone" value={customer.emergencyContactPhone ?? ''} onChange={(e) => setCustomer({ ...customer, emergencyContactPhone: e.target.value })} fullWidth />
                <TextField label="Relation" value={customer.emergencyContactRelation ?? ''} onChange={(e) => setCustomer({ ...customer, emergencyContactRelation: e.target.value })} fullWidth />
              </Stack>

              <Button variant="contained" onClick={handleSave} sx={{ alignSelf: 'flex-start' }}>Save Changes</Button>
              {message ? <Typography color="primary.main">{message}</Typography> : null}
            </Stack>
          </CardContent>
        </Card>

        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Card>
              <CardContent>
                <Stack spacing={1}>
                  <Typography color="text.secondary">Wallet Balance</Typography>
                  <Typography variant="h5">{customer.walletBalance}</Typography>
                  <Button size="small" variant="outlined" onClick={() => setWalletDialog(true)} sx={{ alignSelf: 'flex-start' }}>Adjust Wallet</Button>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6}>
            <Card>
              <CardContent>
                <Stack spacing={1}>
                  <Typography color="text.secondary">Loyalty Points</Typography>
                  <Typography variant="h5">{customer.loyaltyPoints}</Typography>
                  <Button size="small" variant="outlined" onClick={() => setLoyaltyDialog(true)} sx={{ alignSelf: 'flex-start' }}>Adjust Loyalty</Button>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h6">Documents</Typography>
                <Button variant="outlined" onClick={() => setDocDialogOpen(true)}>Upload Document</Button>
              </Stack>
              {documents.map((doc) => (
                <Stack key={doc.id} direction="row" spacing={2} alignItems="center">
                  <Chip label={doc.documentType} size="small" />
                  <Typography variant="body2">{doc.documentNumber ?? '-'}</Typography>
                  <Chip
                    label={doc.verificationStatus}
                    size="small"
                    color={doc.verificationStatus === 'Verified' ? 'success' : doc.verificationStatus === 'Rejected' ? 'error' : 'warning'}
                  />
                  {doc.storagePath ? <Button size="small" href={doc.storagePath} target="_blank" rel="noreferrer">View</Button> : null}
                  <Button size="small" onClick={() => handleVerifyDocument(doc.id, 'Verified')}>Verify</Button>
                  <Button size="small" onClick={() => handleVerifyDocument(doc.id, 'Rejected')}>Reject</Button>
                  <Button size="small" color="error" onClick={() => handleDeleteDocument(doc.id)}>Delete</Button>
                </Stack>
              ))}
              {documents.length === 0 && <Typography color="text.secondary">No documents uploaded.</Typography>}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Rental History</Typography>
              <Table size="small">
                <TableHead>
                  <TableRow><TableCell>Vehicle</TableCell><TableCell>Dates</TableCell><TableCell>Total</TableCell><TableCell>Status</TableCell></TableRow>
                </TableHead>
                <TableBody>
                  {bookings.map((b) => (
                    <TableRow key={b.id}>
                      <TableCell>{b.vehicleId}</TableCell>
                      <TableCell>{new Date(b.startDate).toLocaleDateString()} - {new Date(b.endDate).toLocaleDateString()}</TableCell>
                      <TableCell>{b.totalAmount}</TableCell>
                      <TableCell>{b.status}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              {bookings.length === 0 && <Typography color="text.secondary">No bookings yet.</Typography>}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={1}>
              <Typography variant="h6">Timeline</Typography>
              {timeline.map((entry) => (
                <Stack key={entry.id} direction="row" spacing={2}>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 160 }}>
                    {new Date(entry.createdAt).toLocaleString()}
                  </Typography>
                  <Typography variant="body2">{entry.action}</Typography>
                </Stack>
              ))}
              {timeline.length === 0 && <Typography color="text.secondary">No activity yet.</Typography>}
            </Stack>
          </CardContent>
        </Card>
      </Stack>

      <Dialog open={walletDialog} onClose={() => setWalletDialog(false)}>
        <DialogTitle>Adjust Wallet</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1, minWidth: 320 }}>
          <TextField label="Amount (negative to debit)" type="number" value={walletForm.amount} onChange={(e) => setWalletForm({ ...walletForm, amount: e.target.value })} />
          <TextField label="Reason" value={walletForm.reason} onChange={(e) => setWalletForm({ ...walletForm, reason: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setWalletDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleAdjustWallet}>Apply</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={loyaltyDialog} onClose={() => setLoyaltyDialog(false)}>
        <DialogTitle>Adjust Loyalty Points</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1, minWidth: 320 }}>
          <TextField label="Points (negative to redeem)" type="number" value={loyaltyForm.points} onChange={(e) => setLoyaltyForm({ ...loyaltyForm, points: e.target.value })} />
          <TextField label="Reason" value={loyaltyForm.reason} onChange={(e) => setLoyaltyForm({ ...loyaltyForm, reason: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setLoyaltyDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleAdjustLoyalty}>Apply</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={docDialogOpen} onClose={() => setDocDialogOpen(false)}>
        <DialogTitle>Upload Document</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1, minWidth: 360 }}>
          <TextField select label="Document Type" value={docForm.documentType} onChange={(e) => setDocForm({ ...docForm, documentType: e.target.value })}>
            {DOCUMENT_TYPES.map((type) => <MenuItem key={type} value={type}>{type}</MenuItem>)}
          </TextField>
          <TextField label="Document Number" value={docForm.documentNumber} onChange={(e) => setDocForm({ ...docForm, documentNumber: e.target.value })} />
          <TextField label="Expiry Date" type="date" InputLabelProps={{ shrink: true }} value={docForm.expiryDate} onChange={(e) => setDocForm({ ...docForm, expiryDate: e.target.value })} />
          <Button component="label">
            {docForm.file ? docForm.file.name : 'Choose File'}
            <input type="file" hidden onChange={(e) => setDocForm({ ...docForm, file: e.target.files?.[0] ?? null })} />
          </Button>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDocDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleUploadDocument}>Upload</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
